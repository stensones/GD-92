namespace Stensones.GD92.Fields;

public sealed record AlarmReference : IGD9Field
{
	private const int BitCount = 16;
	private const int ByteBitCount = 8;

	private AlarmReference(ushort value) => this.Value = value;

	public ushort Value { get; }

	public static AlarmReference FromValue(ushort value) => new(value);

	public static AlarmReference FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((ushort)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)(this.Value >> ByteBitCount), (byte)this.Value];
}
