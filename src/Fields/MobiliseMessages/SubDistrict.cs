namespace Stensones.GD92.Fields;

public sealed record SubDistrict : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 30;

	private SubDistrict(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static SubDistrict FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new SubDistrict(value);
	}

	public static SubDistrict FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
