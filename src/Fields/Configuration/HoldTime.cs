namespace Stensones.GD92.Fields;

public sealed record HoldTime : IGD9Field
{
	private const int BitCount = 8;

	private HoldTime(byte value) => this.Value = value;

	public byte Value { get; }

	public static HoldTime FromValue(byte value) => new(value);

	public static HoldTime FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
