namespace Stensones.GD92.Fields;

public sealed record Callsign : IGD9Field
{
	private const int MaximumCallsignLength = 6;

	private Callsign(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static Callsign FromValue(SevenBitAsciiString value)
	{
		if (value.Value.Length > MaximumCallsignLength || value.Value.Any(character => !char.IsAsciiLetterOrDigit(character)))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Callsign(value);
	}

	public static Callsign FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(CountedAsciiStringEncoding.ReadCountedAscii(ref buffer, MaximumCallsignLength, false));
	}

	public byte[] ToWireValue()
	{
		return CountedAsciiStringEncoding.ToCountedWireValue(this.Value, MaximumCallsignLength, false);
	}
}
