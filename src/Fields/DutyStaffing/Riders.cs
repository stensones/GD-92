namespace Stensones.GD92.Fields;

public sealed record Riders : IGD9Field
{
	private const byte MinimumValue = 1;
	private const byte MaximumValue = 15;
	private const int BitCount = 8;

	private Riders(byte value) => this.Value = value;

	public byte Value { get; }

	public static Riders FromValue(byte value)
	{
		if (value is < MinimumValue or > MaximumValue) throw new ArgumentOutOfRangeException(nameof(value));
		return new Riders(value);
	}

	public static Riders FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
