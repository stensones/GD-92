namespace Stensones.GD92.Fields;

public sealed record LanAddress : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 16;

	private LanAddress(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static LanAddress FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new LanAddress(value);
	}

	public static LanAddress FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
