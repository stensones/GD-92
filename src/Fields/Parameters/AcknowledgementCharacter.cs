namespace Stensones.GD92.Fields;

public sealed record AcknowledgementCharacter : IGD9Field
{
	private const int BitCount = 8;
	private const char MaximumAsciiValue = '\u007F';

	private AcknowledgementCharacter(char value) => this.Value = value;

	public char Value { get; }

	public static AcknowledgementCharacter FromValue(char value)
	{
		if (value > MaximumAsciiValue) throw new ArgumentOutOfRangeException(nameof(value));
		return new AcknowledgementCharacter(value);
	}

	public static AcknowledgementCharacter FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((char)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
