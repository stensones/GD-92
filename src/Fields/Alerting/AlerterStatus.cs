namespace Stensones.GD92.Fields;

public sealed record AlerterStatus : IGD9Field
{
	private const int BitCount = 16;
	private const int ByteBitCount = 8;

	private AlerterStatus(AlerterStatusValue value) => this.Value = value;

	public AlerterStatusValue Value { get; }

	public static AlerterStatus FromValue(AlerterStatusValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new AlerterStatus(value);
	}

	public static AlerterStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((AlerterStatusValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)((ushort)this.Value >> ByteBitCount), (byte)this.Value];
}
