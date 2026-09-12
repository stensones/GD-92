namespace Stensones.GD92.Fields;

public sealed record Table : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = byte.MaxValue;

	private Table(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static Table FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new Table(value);
	}

	public static Table FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
