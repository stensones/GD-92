namespace Stensones.GD92.Fields;

public sealed record BlockCheckCharacter : IGD9Field
{
	private const int WordBitCount = 8;
	private const byte InitialBlockCheckValue = 0;

	private BlockCheckCharacter(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static BlockCheckCharacter FromEnvelopeBytes(ReadOnlySpan<byte> envelopeBytes)
	{
		var value = InitialBlockCheckValue;

		foreach (var envelopeByte in envelopeBytes)
		{
			value ^= envelopeByte;
		}

		return new BlockCheckCharacter(value);
	}

	public static BlockCheckCharacter FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new BlockCheckCharacter((byte)buffer.ReadUnsignedBits(WordBitCount));
	}

	public byte[] ToWireValue()
	{
		return [this.Value];
	}
}
