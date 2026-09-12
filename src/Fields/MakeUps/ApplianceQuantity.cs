namespace Stensones.GD92.Fields;

public sealed record ApplianceQuantity : IGD9Field
{
	private const int BitCount = 8;

	private ApplianceQuantity(byte value) => this.Value = value;

	public byte Value { get; }

	public static ApplianceQuantity FromValue(byte value) => new(value);

	public static ApplianceQuantity FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
