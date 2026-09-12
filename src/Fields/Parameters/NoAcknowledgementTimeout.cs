namespace Stensones.GD92.Fields;

public sealed record NoAcknowledgementTimeout : IGD9Field
{
	private NoAcknowledgementTimeout(Word8 value) => this.Value = value;

	public Word8 Value { get; }

	public static NoAcknowledgementTimeout FromValue(Word8 value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return new NoAcknowledgementTimeout(value);
	}

	public static NoAcknowledgementTimeout FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue(Word8.FromEncodedMessageBuffer(ref buffer));

	public byte[] ToWireValue() => this.Value.ToWireValue();
}
