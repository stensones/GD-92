namespace Stensones.GD92.Fields;

public sealed record PagerPriority : IGD9Field
{
	private const int BitCount = 8;

	private PagerPriority(PagerPriorityValue value) => this.Value = value;

	public PagerPriorityValue Value { get; }

	public static PagerPriority FromValue(PagerPriorityValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new PagerPriority(value);
	}

	public static PagerPriority FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((PagerPriorityValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
