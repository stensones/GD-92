namespace Stensones.GD92.Fields;

public sealed record NodeName : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 20;

	private NodeName(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static NodeName FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new NodeName(value);
	}

	public static NodeName FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
