namespace Stensones.GD92.Fields;

public sealed record ManualAcknowledgementRequest : IGD9Field
{
	private ManualAcknowledgementRequest(ProtocolBoolean value)
	{
		this.Value = value;
	}

	public static ManualAcknowledgementRequest Required { get; } = FromValue(ProtocolBoolean.True);
	public static ManualAcknowledgementRequest NotRequired { get; } = FromValue(ProtocolBoolean.False);

	public ProtocolBoolean Value { get; }

	public static ManualAcknowledgementRequest FromValue(ProtocolBoolean value)
	{
		return new ManualAcknowledgementRequest(value);
	}

	public static ManualAcknowledgementRequest FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ProtocolBoolean.FromValue((byte)buffer.ReadUnsignedBits(8)));
	}

	public byte[] ToWireValue()
	{
		return [this.Value.Value];
	}
}
