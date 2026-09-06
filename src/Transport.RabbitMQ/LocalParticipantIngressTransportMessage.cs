using Wolverine;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class LocalParticipantIngressTransportMessage : ISerializable
{
	private readonly byte[] envelopeWireValue;

	public LocalParticipantIngressTransportMessage(byte[] envelopeWireValue)
	{
		ArgumentNullException.ThrowIfNull(envelopeWireValue);

		this.envelopeWireValue = envelopeWireValue.ToArray();
	}

	public byte[] EnvelopeWireValue => this.envelopeWireValue.ToArray();

	public byte[] Write()
	{
		return this.EnvelopeWireValue;
	}

	public static object Read(byte[] data)
	{
		return new LocalParticipantIngressTransportMessage(data);
	}
}
