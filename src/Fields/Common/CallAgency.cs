namespace Stensones.GD92.Fields;

public sealed record CallAgency : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 10;

	private CallAgency(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static CallAgency FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new CallAgency(value);
	}

	public static CallAgency FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
