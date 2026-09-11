namespace Stensones.GD92.Fields;

public sealed record TelephoneNumber : IGD9Field
{
	private const int MaximumTelephoneNumberLength = 16;

	private TelephoneNumber(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static TelephoneNumber FromValue(SevenBitAsciiString value)
	{
		var number = value.Value.TrimEnd(' ');

		if (value.Value.Length > MaximumTelephoneNumberLength || number.Any(character => !char.IsAsciiDigit(character)))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new TelephoneNumber(value);
	}

	public static TelephoneNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(CountedAsciiStringEncoding.ReadCountedAscii(ref buffer, MaximumTelephoneNumberLength, false));
	}

	public byte[] ToWireValue()
	{
		return CountedAsciiStringEncoding.ToCountedWireValue(this.Value, MaximumTelephoneNumberLength, false);
	}
}
