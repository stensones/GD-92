namespace Stensones.GD92.Fields;

public sealed record AlarmSerial : CountedAsciiStringField, IGD9Field
{
	private const int MaximumEncodedLength = 12;

	private AlarmSerial(SevenBitAsciiString value)
		: base(value)
	{
	}

	public static AlarmSerial FromValue(SevenBitAsciiString value)
	{
		ValidateValue(value, MaximumEncodedLength, false);
		return new AlarmSerial(value);
	}

	public static AlarmSerial FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ReadValue(ref buffer, MaximumEncodedLength, false));
	}

	public byte[] ToWireValue()
	{
		return this.GetWireValue(MaximumEncodedLength, false);
	}
}
