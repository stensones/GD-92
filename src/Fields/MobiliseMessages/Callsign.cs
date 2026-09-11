namespace Stensones.GD92.Fields;

public sealed record Callsign : IGD9Field
{
	private Callsign(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static Callsign FromValue(SevenBitAsciiString value)
	{
		if (value.Value.Length > 6 || value.Value.Any(character => !char.IsAsciiLetterOrDigit(character)))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Callsign(value);
	}

	public static Callsign FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 6, false));
	}

	public byte[] ToWireValue()
	{
		return MobiliseMessageStringEncoding.ToCountedWireValue(this.Value, 6, false);
	}
}
