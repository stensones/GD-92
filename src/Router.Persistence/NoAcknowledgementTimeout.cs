using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed record NoAcknowledgementTimeout : IGD9Field
{
	private NoAcknowledgementTimeout(Word8 value)
	{
		this.Value = value;
	}

	public Word8 Value { get; }

	public static NoAcknowledgementTimeout FromValue(Word8 value)
	{
		ArgumentNullException.ThrowIfNull(value);

		return new NoAcknowledgementTimeout(value);
	}

	public static NoAcknowledgementTimeout FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(Word8.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.Value.ToWireValue();
	}
}
