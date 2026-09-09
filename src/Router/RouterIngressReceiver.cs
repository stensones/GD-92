using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace Router;

public sealed class RouterIngressReceiver : IRouterIngressReceiver
{
	private readonly RouterParameterRequestHandler parameterRequestHandler;
	private readonly LocalDeliveryClassification localDeliveryClassification;
	private readonly IUserAgentIngress userAgentIngress;
	private readonly ILocalParticipantIngress localParticipantIngress;
	private readonly ILogger<RouterIngressReceiver> logger;

	public RouterIngressReceiver(
		RouterParameterRequestHandler parameterRequestHandler,
		IUserAgentIngress userAgentIngress,
		ILocalParticipantIngress localParticipantIngress,
		ILogger<RouterIngressReceiver> logger)
	{
		this.parameterRequestHandler = parameterRequestHandler ??
			throw new ArgumentNullException(nameof(parameterRequestHandler));
		this.localDeliveryClassification = new LocalDeliveryClassification(
			this.parameterRequestHandler.LocalAddress);
		this.userAgentIngress = userAgentIngress ??
			throw new ArgumentNullException(nameof(userAgentIngress));
		this.localParticipantIngress = localParticipantIngress ??
			throw new ArgumentNullException(nameof(localParticipantIngress));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		this.logger.LogInformation(
			"Router received Message Type {MessageType} from {Source} for {DestinationCount} destination(s).",
			envelope.Contents.Type.Value,
			envelope.Source,
			envelope.Destinations.Addresses.Count);

		switch (this.localDeliveryClassification.Classify(envelope))
		{
			case LocalDeliveryOutcome.RouterHandling:
			{
				var handling = await this.parameterRequestHandler.HandleAsync(
					envelope,
					cancellationToken);
				var response = handling.Response;

				if (response is not null)
				{
					await this.userAgentIngress.DeliverAsync(response, cancellationToken);
					this.logger.LogInformation(
						"Router returned Message Type {MessageType} to {Destination}.",
						response.Contents.Type.Value,
						response.Destinations.Addresses[0]);
				}
				else
				{
					this.logger.LogWarning(
						"Router did not handle Message Type {MessageType}: {HandlingStatus}.",
						envelope.Contents.Type.Value,
						handling.Status);
				}

				break;
			}

			case LocalDeliveryOutcome.LocalParticipantIngress:
			{
				await this.localParticipantIngress.DeliverAsync(envelope, cancellationToken);
				break;
			}

			case LocalDeliveryOutcome.UserAgentIngress:
			{
				await this.userAgentIngress.DeliverAsync(envelope, cancellationToken);
				break;
			}

			case LocalDeliveryOutcome.NotLocallyDeliverable:
			{
				this.logger.LogWarning(
					"Router did not locally deliver Message Type {MessageType}.",
					envelope.Contents.Type.Value);
				break;
			}
		}
	}
}
