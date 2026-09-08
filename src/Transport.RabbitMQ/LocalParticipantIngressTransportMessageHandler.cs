using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class LocalParticipantIngressTransportMessageHandler
{
	public Task HandleAsync(
		LocalParticipantIngressTransportMessage message,
		ILocalParticipantIngressReceiver receiver,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(receiver);
		cancellationToken.ThrowIfCancellationRequested();

		var envelope = IngressEnvelopeTransport.DecodeEnvelope(message.EnvelopeWireValue);

		return receiver.ReceiveAsync(envelope, cancellationToken);
	}
}
