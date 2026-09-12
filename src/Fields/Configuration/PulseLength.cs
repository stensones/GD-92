namespace Stensones.GD92.Fields;

public sealed record PulseLength : IGD9Field
{
	private const int BitCount = 16;
	private const int ByteBitCount = 8;

	private PulseLength(ushort value) => this.Value = value;

	public ushort Value { get; }

	public static PulseLength FromValue(ushort value) => new(value);

	public static PulseLength FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((ushort)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)(this.Value >> ByteBitCount), (byte)this.Value];
}
