namespace Stensones.GD92.Fields;

public record Word8 : IGD9Field
{
	protected Word8(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static Word8 FromValue(byte value)
	{
		return new Word8(value);
	}

	public static Word8 FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new Word8((byte)buffer.ReadUnsignedBits(8));
	}

	public byte[] ToWireValue()
	{
		return [this.Value];
	}
}
