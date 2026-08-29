namespace Stensones.GD92.Fields;

public sealed record OfBlocks : Word8
{
	private OfBlocks(byte value)
		: base(value)
	{
	}

	public new static OfBlocks FromValue(byte value)
	{
		return new OfBlocks(value);
	}

	public new static OfBlocks FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new OfBlocks((byte)buffer.ReadUnsignedBits(8));
	}
}
