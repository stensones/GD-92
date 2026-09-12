namespace Stensones.GD92.Fields;

public sealed record GenerateAlarm : IGD9Field
{
	private const int BitCount = 8;

	private GenerateAlarm(GenerateAlarmValue value) => this.Value = value;

	public GenerateAlarmValue Value { get; }

	public static GenerateAlarm FromValue(GenerateAlarmValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new GenerateAlarm(value);
	}

	public static GenerateAlarm FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((GenerateAlarmValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
