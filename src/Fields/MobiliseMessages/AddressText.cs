namespace Stensones.GD92.Fields;

public sealed record AddressText : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 120;

	private AddressText(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static AddressText FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new AddressText(value);
	}

	public static AddressText FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
