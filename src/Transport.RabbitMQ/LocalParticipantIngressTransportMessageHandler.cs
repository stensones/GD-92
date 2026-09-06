using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

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

		var buffer = new EncodedMessageBuffer(message.EnvelopeWireValue);
		var envelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		if (buffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException("The encoded local participant ingress Envelope contains trailing bytes.");
		}

		return receiver.ReceiveAsync(envelope, cancellationToken);
	}
}
