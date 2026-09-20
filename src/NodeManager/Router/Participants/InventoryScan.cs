using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Participants;

public sealed class InventoryScan(
	RouterParameterRequestSettings settings,
	InventoryScanSettings inventoryScanSettings,
	IManagementTransactionService managementTransactions,
	IHostApplicationLifetime applicationLifetime,
	IInventoryScanUiNotifier? uiNotifier = null,
	ILogger<InventoryScan>? logger = null)
{
	private const int FirstParticipantPort = 1;
	private const int LastParticipantPort = 63;
	private readonly RouterParameterRequestSettings settings = settings ??
		throw new ArgumentNullException(nameof(settings));
	private readonly InventoryScanSettings inventoryScanSettings = inventoryScanSettings ??
		throw new ArgumentNullException(nameof(inventoryScanSettings));
	private readonly IManagementTransactionService managementTransactions = managementTransactions ??
		throw new ArgumentNullException(nameof(managementTransactions));
	private readonly CancellationToken applicationStopping = applicationLifetime?.ApplicationStopping ??
		throw new ArgumentNullException(nameof(applicationLifetime));
	private readonly IInventoryScanUiNotifier uiNotifier = uiNotifier ?? new NullInventoryScanUiNotifier();
	private readonly ILogger<InventoryScan> logger = logger ?? NullLogger<InventoryScan>.Instance;
	private readonly object synchronizationLock = new();
	private readonly Dictionary<Guid, InventoryScanStatus> statuses = [];
	private readonly Dictionary<Guid, ManagementTransactionUiRecipient> uiRecipients = [];

	public InventoryScanStatusIdentifier Start()
	{
		var identifier = InventoryScanStatusIdentifier.Create();
		lock (this.synchronizationLock)
		{
			this.statuses.Add(identifier.Value, new InventoryScanStatus(
				CompletedProbeCount: 0,
				Participants:
				[
					new InventoryParticipant(
						Port: 0,
						Kind: "router",
						AgentType: null)
				],
				Summary: InventoryScanSummary.Empty));
		}

		_ = this.RunAsync(identifier);
		return identifier;
	}

	public InventoryScanStatus? Get(InventoryScanStatusIdentifier identifier)
	{
		ArgumentNullException.ThrowIfNull(identifier);

		lock (this.synchronizationLock)
		{
			return this.statuses.GetValueOrDefault(identifier.Value);
		}
	}

	public async Task RegisterUiRecipientAsync(
		InventoryScanStatusIdentifier identifier,
		ManagementTransactionUiRecipient recipient,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(identifier);
		ArgumentNullException.ThrowIfNull(recipient);
		cancellationToken.ThrowIfCancellationRequested();

		InventoryScanProgressUpdate update;
		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(identifier.Value, out var status))
			{
				throw new InvalidOperationException("The Inventory Scan does not exist.");
			}

			this.uiRecipients[identifier.Value] = recipient;
			update = CreateProgressUpdate(identifier, recipient, status);
		}

		await this.NotifyUiAsync(update);
	}

	private async Task RunAsync(InventoryScanStatusIdentifier identifier)
	{
		try
		{
			await Parallel.ForEachAsync(
				Enumerable.Range(FirstParticipantPort, LastParticipantPort),
				new ParallelOptions
				{
					CancellationToken = this.applicationStopping,
					MaxDegreeOfParallelism = this.inventoryScanSettings.MaximumConcurrentProbes
				},
				async (port, cancellationToken) =>
				{
					await this.ProbeAsync(identifier, (byte)port, cancellationToken);
				});
		}
		catch (OperationCanceledException) when (this.applicationStopping.IsCancellationRequested)
		{
		}
	}

	private async Task ProbeAsync(
		InventoryScanStatusIdentifier inventoryScanIdentifier,
		byte port,
		CancellationToken cancellationToken)
	{
		var participantAddress = CommunicationsAddress.FromValues(
			this.settings.LocalRouter.Brigade,
			this.settings.LocalRouter.Node,
			Port.FromValue(PortIdentifier.FromValue(port)));
		var statusIdentifier = await this.managementTransactions.SubmitAsync(
			new ManagementTransactionRequest(
				this.settings.MessageOriginator,
				participantAddress,
				sequenceNumber => Envelope.FromValues(
					this.settings.MessageOriginator,
					Destinations.FromAddresses(participantAddress),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						sequenceNumber,
						AcknowledgementRequest.Requested),
					ParameterRequest.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(2))),
				ManagementTransactionKind.ParameterRequest),
			cancellationToken);
		var status = await this.managementTransactions.WaitForCompletionAsync(
			statusIdentifier,
			cancellationToken);

		switch (status)
		{
			case ReceivedRouterParameterRequestStatus receivedResponse:
				await this.RecordParticipantAsync(
					inventoryScanIdentifier,
					port,
					receivedResponse.ParameterValue);
				break;
			case RejectedRouterParameterRequestStatus rejectedResponse:
				await this.RecordNegativeAcknowledgementAsync(
					inventoryScanIdentifier,
					rejectedResponse.ReasonCode);
				break;
			case TimedOutRouterParameterRequestStatus:
				await this.RecordTimeoutAsync(inventoryScanIdentifier);
				break;
			case DeliveryFailedRouterParameterRequestStatus:
				await this.RecordDeliveryFailureAsync(inventoryScanIdentifier);
				break;
		}
	}

	private Task RecordTimeoutAsync(InventoryScanStatusIdentifier identifier)
	{
		return this.UpdateStatusAsync(identifier, status =>
		{
			var summary = status.Summary;
			return status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { TimeoutCount = summary.TimeoutCount + 1 }
			};
		});
	}

	private Task RecordDeliveryFailureAsync(InventoryScanStatusIdentifier identifier)
	{
		return this.UpdateStatusAsync(identifier, status =>
		{
			var summary = status.Summary;
			return status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { DeliveryFailureCount = summary.DeliveryFailureCount + 1 }
			};
		});
	}

	private Task RecordNegativeAcknowledgementAsync(
		InventoryScanStatusIdentifier identifier,
		ReasonCode reasonCode)
	{
		ArgumentNullException.ThrowIfNull(reasonCode);

		return this.UpdateStatusAsync(identifier, status =>
		{
			var summary = status.Summary;
			var reasonCodeKey = FormatReasonCode(reasonCode);
			var negativeAcknowledgements = summary.NegativeAcknowledgements
				.ToDictionary(entry => entry.Key, entry => entry.Value);
			negativeAcknowledgements[reasonCodeKey] =
				negativeAcknowledgements.GetValueOrDefault(reasonCodeKey) + 1;

			return status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { NegativeAcknowledgements = negativeAcknowledgements }
			};
		});
	}

	private Task RecordParticipantAsync(
		InventoryScanStatusIdentifier identifier,
		byte port,
		ParameterValue agentType)
	{
		ArgumentNullException.ThrowIfNull(agentType);

		var participant = AgentTypeInventoryParticipant.FromParameterValue(port, agentType);
		return this.UpdateStatusAsync(identifier, status =>
		{
			var summary = status.Summary;
			return status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Participants = [.. status.Participants, participant],
				Summary = summary with
				{
					DiscoveredParticipantCount = summary.DiscoveredParticipantCount + 1
				}
			};
		});
	}

	private async Task UpdateStatusAsync(
		InventoryScanStatusIdentifier identifier,
		Func<InventoryScanStatus, InventoryScanStatus> update)
	{
		InventoryScanProgressUpdate? progressUpdate;
		lock (this.synchronizationLock)
		{
			var status = this.statuses.GetValueOrDefault(identifier.Value) ??
				throw new InvalidOperationException("The Inventory Scan does not exist.");
			var updatedStatus = update(status);
			this.statuses[identifier.Value] = updatedStatus;
			progressUpdate = this.uiRecipients.TryGetValue(identifier.Value, out var recipient)
				? CreateProgressUpdate(identifier, recipient, updatedStatus)
				: null;
		}

		if (progressUpdate is not null)
		{
			await this.NotifyUiAsync(progressUpdate);
		}
	}

	private static InventoryScanProgressUpdate CreateProgressUpdate(
		InventoryScanStatusIdentifier identifier,
		ManagementTransactionUiRecipient recipient,
		InventoryScanStatus status)
	{
		return new InventoryScanProgressUpdate(
			recipient.BrowserSessionIdentifier,
			recipient.RequestIdentifier,
			identifier.ToString(),
			status);
	}

	private async Task NotifyUiAsync(InventoryScanProgressUpdate update)
	{
		try
		{
			await this.uiNotifier.NotifyAsync(update, CancellationToken.None);
		}
		catch (Exception exception)
		{
			this.logger.LogWarning(
				exception,
				"Could not notify the browser that Inventory Scan {ScanIdentifier} progressed.",
				update.ScanIdentifier);
		}
	}

	private static string FormatReasonCode(ReasonCode reasonCode)
	{
		return reasonCode switch
		{
			{ ParameterReasonCode: { } parameterReasonCode } =>
				$"parameter:{ToSnakeCase(parameterReasonCode.ToString())}",
			{ GeneralReasonCode: { } generalReasonCode } =>
				$"general:{ToSnakeCase(generalReasonCode.ToString())}",
			_ => throw new InvalidOperationException("The Reason Code set is not supported.")
		};
	}

	private static string ToSnakeCase(string value)
	{
		return string.Concat(value.Select((character, index) =>
			index > 0 && char.IsUpper(character)
				? $"_{char.ToLowerInvariant(character)}"
				: char.ToLowerInvariant(character).ToString()));
	}
}
