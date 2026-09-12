namespace Stensones.GD92.Fields;

public sealed record PagerText : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 200;

	private PagerText(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static PagerText FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new PagerText(value);
	}

	public static PagerText FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
