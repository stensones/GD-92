namespace Stensones.GD92.Fields;

public sealed record OfficerInCharge : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 20;

	private OfficerInCharge(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static OfficerInCharge FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new OfficerInCharge(value);
	}

	public static OfficerInCharge FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
