using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Participants;

public sealed class InventoryScan(
	RouterParameterRequestSettings settings,
	InventoryScanSettings inventoryScanSettings,
	IManagementTransactionService managementTransactions,
	IHostApplicationLifetime applicationLifetime)
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
	private readonly object synchronizationLock = new();
	private readonly Dictionary<Guid, InventoryScanStatus> statuses = [];

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
				this.RecordParticipant(inventoryScanIdentifier, port, receivedResponse.ParameterValue);
				break;
			case RejectedRouterParameterRequestStatus rejectedResponse:
				this.RecordNegativeAcknowledgement(inventoryScanIdentifier, rejectedResponse.ReasonCode);
				break;
			case TimedOutRouterParameterRequestStatus:
				this.RecordTimeout(inventoryScanIdentifier);
				break;
			case DeliveryFailedRouterParameterRequestStatus:
				this.RecordDeliveryFailure(inventoryScanIdentifier);
				break;
		}
	}

	private void RecordTimeout(InventoryScanStatusIdentifier identifier)
	{
		this.UpdateStatus(identifier, status =>
		{
			var summary = status.Summary;
			return status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { TimeoutCount = summary.TimeoutCount + 1 }
			};
		});
	}

	private void RecordDeliveryFailure(InventoryScanStatusIdentifier identifier)
	{
		this.UpdateStatus(identifier, status =>
		{
			var summary = status.Summary;
			return status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { DeliveryFailureCount = summary.DeliveryFailureCount + 1 }
			};
		});
	}

	private void RecordNegativeAcknowledgement(
		InventoryScanStatusIdentifier identifier,
		ReasonCode reasonCode)
	{
		ArgumentNullException.ThrowIfNull(reasonCode);

		this.UpdateStatus(identifier, status =>
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

	private void RecordParticipant(
		InventoryScanStatusIdentifier identifier,
		byte port,
		ParameterValue agentType)
	{
		ArgumentNullException.ThrowIfNull(agentType);

		var participant = AgentTypeInventoryParticipant.FromParameterValue(port, agentType);
		this.UpdateStatus(identifier, status =>
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

	private void UpdateStatus(
		InventoryScanStatusIdentifier identifier,
		Func<InventoryScanStatus, InventoryScanStatus> update)
	{
		lock (this.synchronizationLock)
		{
			var status = this.statuses.GetValueOrDefault(identifier.Value) ??
				throw new InvalidOperationException("The Inventory Scan does not exist.");
			this.statuses[identifier.Value] = update(status);
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
