namespace Stensones.GD92.Fields;

public sealed record AvlType : IGD9Field
{
	private const int BitCount = 8;

	private AvlType(AvlTypeValue value) => this.Value = value;

	public AvlTypeValue Value { get; }

	public static AvlType FromValue(AvlTypeValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new AvlType(value);
	}

	public static AvlType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((AvlTypeValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
