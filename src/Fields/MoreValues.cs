namespace Stensones.GD92.Fields;

public sealed record MoreValues : Word8
{
	private MoreValues(byte value)
		: base(value)
	{
	}

	public static MoreValues No { get; } = FromValue(0);
	public static MoreValues Yes { get; } = FromValue(1);

	public new static MoreValues FromValue(byte value)
	{
		if (value > 1)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MoreValues(value);
	}

	public new static MoreValues FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((byte)buffer.ReadUnsignedBits(8));
	}
}
