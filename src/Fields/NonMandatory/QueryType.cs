namespace Stensones.GD92.Fields;

public sealed record QueryType : IGD9Field
{
	private const int BitCount = 8;

	private QueryType(byte value) => this.Value = value;

	public byte Value { get; }

	public static QueryType FromValue(byte value) => new(value);

	public static QueryType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
