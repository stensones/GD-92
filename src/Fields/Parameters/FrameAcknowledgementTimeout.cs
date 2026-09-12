namespace Stensones.GD92.Fields;

public sealed record FrameAcknowledgementTimeout : IGD9Field
{
	private const int BitCount = 8;

	private FrameAcknowledgementTimeout(byte value) => this.Value = value;

	public byte Value { get; }

	public static FrameAcknowledgementTimeout FromValue(byte value) => new(value);

	public static FrameAcknowledgementTimeout FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
