using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

internal sealed class IngressEnvelopeTransport(IMessageBus messageBus)
{
	private readonly IMessageBus messageBus = messageBus ??
		throw new ArgumentNullException(nameof(messageBus));

	public async Task SubmitAsync(
		Envelope envelope,
		Uri endpoint,
		Func<byte[], object> createTransportMessage,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		ArgumentNullException.ThrowIfNull(endpoint);
		ArgumentNullException.ThrowIfNull(createTransportMessage);

		var message = createTransportMessage(envelope.ToWireValue());
		await this.messageBus
			.EndpointFor(endpoint)
			.SendAsync(message)
			.AsTask()
			.WaitAsync(cancellationToken);
	}

	public static Envelope DecodeEnvelope(byte[] wireValue)
	{
		ArgumentNullException.ThrowIfNull(wireValue);

		var buffer = new EncodedMessageBuffer(wireValue);
		var envelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		if (buffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException("The encoded ingress Envelope contains trailing bytes.");
		}

		return envelope;
	}
}
