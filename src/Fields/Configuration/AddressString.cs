namespace Stensones.GD92.Fields;

public sealed record AddressString : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 30;

	private AddressString(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static AddressString FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new AddressString(value);
	}

	public static AddressString FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
