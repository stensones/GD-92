namespace Stensones.GD92.Fields;

public sealed record NumberTypes : IGD9Field
{
	private const byte MaximumValue = 12;
	private const int BitCount = 8;

	private NumberTypes(byte value) => this.Value = value;

	public byte Value { get; }

	public static NumberTypes FromValue(byte value)
	{
		if (value > MaximumValue) throw new ArgumentOutOfRangeException(nameof(value));
		return new NumberTypes(value);
	}

	public static NumberTypes FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
