using Wolverine;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class UserAgentIngressTransportMessage : ISerializable
{
	private readonly byte[] envelopeWireValue;

	public UserAgentIngressTransportMessage(byte[] envelopeWireValue)
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
		return new UserAgentIngressTransportMessage(data);
	}
}
