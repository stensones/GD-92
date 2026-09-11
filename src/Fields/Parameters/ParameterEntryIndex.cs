namespace Stensones.GD92.Fields;

public sealed record ParameterEntryIndex : IGD9Field
{
	private const int WordBitCount = 16;
	private const int ByteBitCount = 8;

	private ParameterEntryIndex(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static ParameterEntryIndex FromValue(ushort value)
	{
		return new ParameterEntryIndex(value);
	}

	public static ParameterEntryIndex FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((ushort)buffer.ReadUnsignedBits(WordBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)(this.Value >> ByteBitCount), (byte)this.Value];
	}
}
