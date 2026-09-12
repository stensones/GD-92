namespace Stensones.GD92.Fields;

public abstract record CountedAsciiStringField
{
	protected CountedAsciiStringField(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	protected static SevenBitAsciiString ReadValue(
		ref EncodedMessageBuffer buffer,
		int maximumEncodedLength,
		bool isCompressed)
	{
		return CountedAsciiStringEncoding.ReadCountedAscii(ref buffer, maximumEncodedLength, isCompressed);
	}

	protected static void ValidateValue(
		SevenBitAsciiString value,
		int maximumEncodedLength,
		bool isCompressed)
	{
		_ = CountedAsciiStringEncoding.ToCountedWireValue(value, maximumEncodedLength, isCompressed);
	}

	protected byte[] GetWireValue(int maximumEncodedLength, bool isCompressed)
	{
		return CountedAsciiStringEncoding.ToCountedWireValue(this.Value, maximumEncodedLength, isCompressed);
	}
}
