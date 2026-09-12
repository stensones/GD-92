namespace Stensones.GD92.Fields;

public sealed record FrameDuration : IGD9Field
{
	private const int BitCount = 8;

	private FrameDuration(byte value) => this.Value = value;

	public byte Value { get; }

	public static FrameDuration FromValue(byte value) => new(value);

	public static FrameDuration FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
