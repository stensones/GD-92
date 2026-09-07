using Microsoft.Extensions.DependencyInjection;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Participants;

public sealed class InventoryScanRunner(
	RouterParameterRequestSettings settings,
	IInventoryScanRegistry inventoryScans,
	IPendingDeliveryRegistry pendingDeliveries,
	IServiceScopeFactory serviceScopeFactory,
	INodeLoginRetryDelay retryDelay,
	IHostApplicationLifetime applicationLifetime) : IInventoryScanRunner
{
	private const int FirstParticipantPort = 1;
	private const int LastParticipantPort = 63;
	private const int MaximumConcurrentProbes = 8;
	private readonly RouterParameterRequestSettings settings = settings ??
		throw new ArgumentNullException(nameof(settings));
	private readonly IInventoryScanRegistry inventoryScans = inventoryScans ??
		throw new ArgumentNullException(nameof(inventoryScans));
	private readonly IPendingDeliveryRegistry pendingDeliveries = pendingDeliveries ??
		throw new ArgumentNullException(nameof(pendingDeliveries));
	private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ??
		throw new ArgumentNullException(nameof(serviceScopeFactory));
	private readonly INodeLoginRetryDelay retryDelay = retryDelay ??
		throw new ArgumentNullException(nameof(retryDelay));
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
		var statusIdentifier = this.pendingDeliveries.Reserve(
			this.settings.MessageOriginator,
			participantAddress);
		var envelope = Envelope.FromValues(
			this.settings.MessageOriginator,
			Destinations.FromAddresses(participantAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				statusIdentifier.USWR.SequenceNumber,
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(2)));

		for (var send = 0; send < this.settings.NodeLoginRetryPolicy.TotalSends.Value.Value; send++)
		{
			using var scope = this.serviceScopeFactory.CreateScope();
			var routerIngress = scope.ServiceProvider.GetRequiredService<IRouterIngress>();
			await routerIngress.SubmitAsync(envelope, cancellationToken);
			await this.retryDelay.WaitAsync(
				this.settings.NodeLoginRetryPolicy.NoAcknowledgementTimeout,
				cancellationToken);

			if (!this.pendingDeliveries.IsPending(statusIdentifier))
			{
				if (this.pendingDeliveries.GetStatus(statusIdentifier) is
					ReceivedRouterParameterRequestStatus receivedResponse)
				{
					this.inventoryScans.RecordParticipant(
						inventoryScanIdentifier,
						port,
						receivedResponse.ParameterValue);
				}
				else if (this.pendingDeliveries.GetStatus(statusIdentifier) is
					RejectedRouterParameterRequestStatus rejectedResponse)
				{
					this.inventoryScans.RecordNegativeAcknowledgement(
						inventoryScanIdentifier,
						rejectedResponse.ReasonCode);
				}

				return;
			}
		}

		if (this.pendingDeliveries.TryTimeoutParameterRequest(statusIdentifier))
		{
			this.inventoryScans.RecordTimeout(inventoryScanIdentifier);
		}
	}
}
