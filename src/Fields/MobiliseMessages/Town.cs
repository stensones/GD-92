namespace Stensones.GD92.Fields;

public sealed record Town : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 30;

	private Town(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static Town FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new Town(value);
	}

	public static Town FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
