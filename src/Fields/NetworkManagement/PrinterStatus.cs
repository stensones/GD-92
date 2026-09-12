namespace Stensones.GD92.Fields;

public sealed record PrinterStatus : IGD9Field
{
	private const int BitCount = 8;

	private PrinterStatus(PrinterStatusValue value) => this.Value = value;

	public PrinterStatusValue Value { get; }

	public static PrinterStatus FromValue(PrinterStatusValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new PrinterStatus(value);
	}

	public static PrinterStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((PrinterStatusValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
