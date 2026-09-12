namespace Stensones.GD92.Fields;

public sealed record RoutingPreference : IGD9Field
{
	private const int BitCount = 8;

	private RoutingPreference(byte value) => this.Value = value;

	public byte Value { get; }

	public static RoutingPreference FromValue(byte value) => new(value);

	public static RoutingPreference FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((byte)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [this.Value];
}
