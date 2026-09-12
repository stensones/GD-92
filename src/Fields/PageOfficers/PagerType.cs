namespace Stensones.GD92.Fields;

public sealed record PagerType : IGD9Field
{
	private const int BitCount = 8;

	private PagerType(PagerTypeValue value) => this.Value = value;

	public PagerTypeValue Value { get; }

	public static PagerType FromValue(PagerTypeValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new PagerType(value);
	}

	public static PagerType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((PagerTypeValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
