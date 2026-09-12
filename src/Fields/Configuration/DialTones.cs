namespace Stensones.GD92.Fields;

public sealed record DialTones : IGD9Field
{
	private const int BitCount = 8;

	private DialTones(DialTonesValue value) => this.Value = value;

	public DialTonesValue Value { get; }

	public static DialTones FromValue(DialTonesValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new DialTones(value);
	}

	public static DialTones FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((DialTonesValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
