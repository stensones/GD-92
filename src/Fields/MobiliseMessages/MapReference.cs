namespace Stensones.GD92.Fields;

public sealed record MapReference : IGD9Field
{
	private MapReference(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static MapReference FromValue(SevenBitAsciiString value)
	{
		if (value.Value.Length > 16)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MapReference(value);
	}

	public static MapReference FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, 16, false));
	}

	public byte[] ToWireValue()
	{
		return MobiliseMessageStringEncoding.ToCountedWireValue(this.Value, 16, false);
	}
}
