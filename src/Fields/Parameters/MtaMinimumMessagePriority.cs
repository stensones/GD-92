namespace Stensones.GD92.Fields;

public sealed record MtaMinimumMessagePriority : IGD9Field
{
	private const int BitCount = 8;

	private MtaMinimumMessagePriority(MessagePriorityLevel value) => this.Value = value;

	public MessagePriorityLevel Value { get; }

	public static MtaMinimumMessagePriority FromValue(MessagePriorityLevel value) => new(value);

	public static MtaMinimumMessagePriority FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue(MessagePriorityLevel.FromValue((byte)buffer.ReadUnsignedBits(BitCount)));

	public byte[] ToWireValue() => [this.Value.Value];
}
