namespace Stensones.GD92.Fields;

public sealed record RetriesAllowed : IGD9Field
{
	private const int BitCount = 8;

	private RetriesAllowed(byte value) => this.Value = value;

	public byte Value { get; }

	public static RetriesAllowed FromValue(byte value) => new(value);

	public static RetriesAllowed FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
