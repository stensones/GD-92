namespace Stensones.GD92.Fields;

public sealed record Postcode : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 10;

	private Postcode(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static Postcode FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new Postcode(value);
	}

	public static Postcode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
