namespace Stensones.GD92.Fields;

public sealed record AlerterEngineering : IGD9Field
{
	private const int BitCount = 8;

	private AlerterEngineering(AlerterEngineeringValue value) => this.Value = value;

	public AlerterEngineeringValue Value { get; }

	public static AlerterEngineering FromValue(AlerterEngineeringValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new AlerterEngineering(value);
	}

	public static AlerterEngineering FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((AlerterEngineeringValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
