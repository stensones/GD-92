namespace Stensones.GD92.Fields;

public sealed record TestType : IGD9Field
{
	private const int BitCount = 8;

	private TestType(byte value) => this.Value = value;

	public byte Value { get; }

	public static TestType FromValue(byte value) => new(value);

	public static TestType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
