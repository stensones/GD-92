using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RabbitMqLocalParticipantIngress(IMessageBus messageBus) : ILocalParticipantIngress
{
	private readonly IngressEnvelopeTransport transport = new(messageBus);

	public async Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1)
		{
			throw new ArgumentException(
				"A Local Participant ingress Envelope must have exactly one destination.",
				nameof(envelope));
		}

		await this.transport.SubmitAsync(
			envelope,
			LocalParticipantIngressEndpoint.From(envelope.Destinations.Addresses[0]),
			wireValue => new LocalParticipantIngressTransportMessage(wireValue),
			cancellationToken);
	}
}
