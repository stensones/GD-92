namespace Stensones.GD92.Fields;

public sealed record FrameReceiveFailureCount : IGD9Field
{
	private const int BitCount = 16;
	private const int ByteBitCount = 8;

	private FrameReceiveFailureCount(ushort value) => this.Value = value;

	public ushort Value { get; }

	public static FrameReceiveFailureCount FromValue(ushort value) => new(value);

	public static FrameReceiveFailureCount FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((ushort)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)(this.Value >> ByteBitCount), (byte)this.Value];
}
