namespace Stensones.GD92.Fields;

public sealed record MessageType : Word8
{
	private const int WordBitCount = 8;

	private MessageType(byte value)
		: base(value)
	{
	}

	public static MessageType FromValue(GD92MessageType value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MessageType((byte)value);
	}

	public new static MessageType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((GD92MessageType)buffer.ReadUnsignedBits(WordBitCount));
	}
}
