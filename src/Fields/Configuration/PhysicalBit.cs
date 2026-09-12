namespace Stensones.GD92.Fields;

public sealed record PhysicalBit : IGD9Field
{
	private const int BitCount = 8;
	private const byte MaximumValue = 15;

	private PhysicalBit(byte value) => this.Value = value;

	public byte Value { get; }

	public static PhysicalBit FromValue(byte value)
	{
		if (value > MaximumValue) throw new ArgumentOutOfRangeException(nameof(value));
		return new PhysicalBit(value);
	}

	public static PhysicalBit FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
