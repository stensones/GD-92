namespace Stensones.GD92.Fields;

public sealed record NetworkUserAddress : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 14;

	private NetworkUserAddress(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static NetworkUserAddress FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new NetworkUserAddress(value);
	}

	public static NetworkUserAddress FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
