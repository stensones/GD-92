using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RabbitMqUserAgentIngress(IMessageBus messageBus) : IUserAgentIngress
{
	private readonly IMessageBus messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

	public async Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1)
		{
			throw new ArgumentException("A User-Agent ingress Envelope must have exactly one destination.", nameof(envelope));
		}

		var message = new UserAgentIngressTransportMessage(envelope.ToWireValue());
		await this.messageBus
			.EndpointFor(UserAgentIngressEndpoint.From(envelope.Destinations.Addresses[0]))
			.SendAsync(message)
			.AsTask()
			.WaitAsync(cancellationToken);
	}
}
