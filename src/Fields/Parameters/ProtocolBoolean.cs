namespace Stensones.GD92.Fields;

public readonly record struct ProtocolBoolean : IGD9Field
{
	private const int BitCount = 8;
	private const byte FalseWireValue = 0;
	private const byte TrueWireValue = 1;

	private ProtocolBoolean(bool value)
	{
		this.Value = value;
	}

	public static ProtocolBoolean False { get; } = new(false);
	public static ProtocolBoolean True { get; } = new(true);

	public bool Value { get; }

	public static ProtocolBoolean FromValue(bool value)
	{
		return value ? True : False;
	}

	public static ProtocolBoolean FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return buffer.ReadUnsignedBits(BitCount) switch
		{
			FalseWireValue => False,
			TrueWireValue => True,
			_ => throw new ArgumentOutOfRangeException(nameof(buffer))
		};
	}

	public byte[] ToWireValue() => [this.Value ? TrueWireValue : FalseWireValue];
}
