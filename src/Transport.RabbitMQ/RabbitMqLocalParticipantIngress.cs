using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RabbitMqLocalParticipantIngress(IMessageBus messageBus) : ILocalParticipantIngress
{
	private readonly IMessageBus messageBus = messageBus ??
		throw new ArgumentNullException(nameof(messageBus));

	public async Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		var message = new LocalParticipantIngressTransportMessage(envelope.ToWireValue());
		await this.messageBus
			.EndpointFor(LocalParticipantIngressEndpoint.From(envelope.Destinations.Addresses[0]))
			.SendAsync(message)
			.AsTask()
			.WaitAsync(cancellationToken);
	}
}
