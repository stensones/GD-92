namespace Stensones.GD92.Fields;

public sealed record PasswordLevel : IGD9Field
{
	private const int WordBitCount = 8;

	private PasswordLevel(PasswordLevelNumber value)
	{
		this.Value = value;
	}

	public PasswordLevelNumber Value { get; }

	public static PasswordLevel FromValue(PasswordLevelNumber value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new PasswordLevel(value);
	}

	public static PasswordLevel FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((PasswordLevelNumber)buffer.ReadUnsignedBits(WordBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)this.Value];
	}
}
