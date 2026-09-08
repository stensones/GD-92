using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class UserAgentIngressTransportMessageHandler
{
	public Task HandleAsync(
		UserAgentIngressTransportMessage message,
		IUserAgentIngressReceiver receiver,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(receiver);
		cancellationToken.ThrowIfCancellationRequested();

		var envelope = IngressEnvelopeTransport.DecodeEnvelope(message.EnvelopeWireValue);

		return receiver.ReceiveAsync(envelope, cancellationToken);
	}
}
