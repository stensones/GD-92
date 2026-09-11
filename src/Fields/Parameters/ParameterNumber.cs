namespace Stensones.GD92.Fields;

public sealed record ParameterNumber : Word8
{
	private const int WordBitCount = 8;

	private ParameterNumber(byte value)
		: base(value)
	{
	}

	public new static ParameterNumber FromValue(byte value)
	{
		return new ParameterNumber(value);
	}

	public new static ParameterNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((byte)buffer.ReadUnsignedBits(WordBitCount));
	}
}
