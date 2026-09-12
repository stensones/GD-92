namespace Stensones.GD92.Fields;

public sealed record FormatType : IGD9Field
{
	private const int BitCount = 8;

	private FormatType(FormatTypeValue value) => this.Value = value;

	public FormatTypeValue Value { get; }

	public static FormatType FromValue(FormatTypeValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new FormatType(value);
	}

	public static FormatType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((FormatTypeValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
