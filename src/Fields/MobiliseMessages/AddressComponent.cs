namespace Stensones.GD92.Fields;

public abstract record AddressComponent
{
	protected AddressComponent(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	protected static SevenBitAsciiString ReadValue(
		ref EncodedMessageBuffer buffer,
		int maximumEncodedLength,
		bool isCompressed)
	{
		return MobiliseMessageStringEncoding.ReadCountedAscii(ref buffer, maximumEncodedLength, isCompressed);
	}

	protected static void ValidateValue(
		SevenBitAsciiString value,
		int maximumEncodedLength,
		bool isCompressed)
	{
		_ = MobiliseMessageStringEncoding.ToCountedWireValue(value, maximumEncodedLength, isCompressed);
	}

	protected byte[] GetWireValue(int maximumEncodedLength, bool isCompressed)
	{
		return MobiliseMessageStringEncoding.ToCountedWireValue(this.Value, maximumEncodedLength, isCompressed);
	}
}
