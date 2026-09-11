namespace Stensones.GD92.Fields;

public sealed record District : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 30;

	private District(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static District FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new District(value);
	}

	public static District FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
