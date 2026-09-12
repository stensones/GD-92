namespace Stensones.GD92.Fields;

public sealed record AvlData : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 40;

	private AvlData(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static AvlData FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new AvlData(value);
	}

	public static AvlData FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
