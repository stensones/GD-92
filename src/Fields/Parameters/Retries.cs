namespace Stensones.GD92.Fields;

public sealed record Retries : IGD9Field
{
	private Retries(Word8 value) => this.Value = value;

	public Word8 Value { get; }

	public static Retries FromValue(Word8 value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return new Retries(value);
	}

	public static Retries FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue(Word8.FromEncodedMessageBuffer(ref buffer));

	public byte[] ToWireValue() => this.Value.ToWireValue();
}
