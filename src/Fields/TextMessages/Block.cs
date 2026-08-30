namespace Stensones.GD92.Fields;

public sealed record Block : Word8
{
	private Block(byte value)
		: base(value)
	{
	}

	public new static Block FromValue(byte value)
	{
		return new Block(value);
	}

	public new static Block FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new Block((byte)buffer.ReadUnsignedBits(8));
	}
}
