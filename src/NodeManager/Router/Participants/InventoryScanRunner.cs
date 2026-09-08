using Microsoft.Extensions.DependencyInjection;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Participants;

public sealed class InventoryScanRunner(
	RouterParameterRequestSettings settings,
	IInventoryScanRegistry inventoryScans,
	IServiceScopeFactory serviceScopeFactory,
	IHostApplicationLifetime applicationLifetime) : IInventoryScanRunner
{
	private const int FirstParticipantPort = 1;
	private const int LastParticipantPort = 63;
	private const int MaximumConcurrentProbes = 8;
	private readonly RouterParameterRequestSettings settings = settings ??
		throw new ArgumentNullException(nameof(settings));
	private readonly IInventoryScanRegistry inventoryScans = inventoryScans ??
		throw new ArgumentNullException(nameof(inventoryScans));
	private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ??
		throw new ArgumentNullException(nameof(serviceScopeFactory));
	private readonly CancellationToken applicationStopping = applicationLifetime?.ApplicationStopping ??
		throw new ArgumentNullException(nameof(applicationLifetime));

	public InventoryScanStatusIdentifier Start()
	{
		var identifier = this.inventoryScans.Start();
		_ = this.RunAsync(identifier);
		return identifier;
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
					MaxDegreeOfParallelism = MaximumConcurrentProbes
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
		using var scope = this.serviceScopeFactory.CreateScope();
		var managementTransactions = scope.ServiceProvider
			.GetRequiredService<IManagementTransactionService>();
		var statusIdentifier = await managementTransactions.SubmitAsync(
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
		var status = await managementTransactions.WaitForCompletionAsync(
			statusIdentifier,
			cancellationToken);

		if (status is ReceivedRouterParameterRequestStatus receivedResponse)
		{
			this.inventoryScans.RecordParticipant(
				inventoryScanIdentifier,
				port,
				receivedResponse.ParameterValue);
		}
		else if (status is RejectedRouterParameterRequestStatus rejectedResponse)
		{
			this.inventoryScans.RecordNegativeAcknowledgement(
				inventoryScanIdentifier,
				rejectedResponse.ReasonCode);
		}
		else if (status is TimedOutRouterParameterRequestStatus)
		{
			this.inventoryScans.RecordTimeout(inventoryScanIdentifier);
		}
		else if (status is DeliveryFailedRouterParameterRequestStatus)
		{
			this.inventoryScans.RecordDeliveryFailure(inventoryScanIdentifier);
		}
	}
}
