namespace Stensones.GD92.Fields;

public sealed record TelephoneNumber : IGD9Field
{
	private TelephoneNumber(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static TelephoneNumber FromValue(SevenBitAsciiString value)
	{
		var number = value.Value.TrimEnd(' ');

		if (value.Value.Length > 16 || number.Any(character => !char.IsAsciiDigit(character)))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new TelephoneNumber(value);
	}

	public static TelephoneNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 16, false));
	}

	public byte[] ToWireValue()
	{
		return MobiliseMessageStringEncoding.ToCountedWireValue(this.Value, 16, false);
	}
}
