using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RabbitMqUserAgentIngress(IMessageBus messageBus) : IUserAgentIngress
{
	private readonly IngressEnvelopeTransport transport = new(messageBus);

	public async Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1)
		{
			throw new ArgumentException("A User-Agent ingress Envelope must have exactly one destination.", nameof(envelope));
		}

		await this.transport.SubmitAsync(
			envelope,
			UserAgentIngressEndpoint.From(envelope.Destinations.Addresses[0]),
			wireValue => new UserAgentIngressTransportMessage(wireValue),
			cancellationToken);
	}
}
