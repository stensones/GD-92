using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace Router;

public sealed class RouterIngressReceiver : IRouterIngressReceiver
{
	private readonly RouterLocalDelivery localDelivery;
	private readonly ILogger<RouterIngressReceiver> logger;

	internal RouterIngressReceiver(
		RouterLocalDelivery localDelivery,
		ILogger<RouterIngressReceiver> logger)
	{
		this.localDelivery = localDelivery ?? throw new ArgumentNullException(nameof(localDelivery));
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

		await this.localDelivery.ReceiveAsync(envelope, cancellationToken);
	}
}
