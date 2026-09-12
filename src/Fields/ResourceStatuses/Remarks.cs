namespace Stensones.GD92.Fields;

public sealed record Remarks : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 200;

	private Remarks(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static Remarks FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new Remarks(value);
	}

	public static Remarks FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
