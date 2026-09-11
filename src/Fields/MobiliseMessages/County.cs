namespace Stensones.GD92.Fields;

public sealed record County : AddressComponent, IGD9Field
{
	private const int MaximumEncodedLength = 20;

	private County(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static County FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new County(value);
	}

	public static County FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
