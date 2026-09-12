namespace Stensones.GD92.Fields;

public sealed record WanAddress : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 16;

	private WanAddress(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static WanAddress FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new WanAddress(value);
	}

	public static WanAddress FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
