namespace Stensones.GD92.Fields;

public sealed record HouseNumber : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 10;

	private HouseNumber(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static HouseNumber FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new HouseNumber(value);
	}

	public static HouseNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
