namespace Stensones.GD92.Fields;

public sealed record MoreValues : IGD9Field
{
	private const int WordBitCount = 8;

	private MoreValues(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static MoreValues No { get; } = FromValue(ProtocolBoolean.False);
	public static MoreValues Yes { get; } = FromValue(ProtocolBoolean.True);

	public static MoreValues FromValue(ProtocolBoolean value)
	{
		return new MoreValues(value.Value);
	}

	public static MoreValues FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ProtocolBoolean.FromValue((byte)buffer.ReadUnsignedBits(WordBitCount)));
	}

	public byte[] ToWireValue()
	{
		return [this.Value];
	}
}
