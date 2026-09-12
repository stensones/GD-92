namespace Stensones.GD92.Fields;

public sealed record Update : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = byte.MaxValue;

	private Update(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static Update FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, true);
		return new Update(value);
	}

	public static Update FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, true));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, true);
	}
}
