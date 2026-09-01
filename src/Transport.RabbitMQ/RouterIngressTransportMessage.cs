using Wolverine;

namespace Stensones.GD92.Transport.RabbitMQ;

internal sealed class RouterIngressTransportMessage : ISerializable
{
	private readonly byte[] envelopeWireValue;

	public RouterIngressTransportMessage(byte[] envelopeWireValue)
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
		return new RouterIngressTransportMessage(data);
	}
}
