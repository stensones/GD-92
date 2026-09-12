namespace Stensones.GD92.Fields;

public sealed record ConnectType : IGD9Field
{
	private const int BitCount = 8;

	private ConnectType(ConnectTypeValue value) => this.Value = value;

	public ConnectTypeValue Value { get; }

	public static ConnectType FromValue(ConnectTypeValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new ConnectType(value);
	}

	public static ConnectType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((ConnectTypeValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
