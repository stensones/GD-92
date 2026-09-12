namespace Stensones.GD92.Fields;

public sealed record AlarmType : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 10;

	private AlarmType(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static AlarmType FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new AlarmType(value);
	}

	public static AlarmType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
