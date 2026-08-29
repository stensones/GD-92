namespace Stensones.GD92.Fields;

public sealed record BlockCheckCharacter : IGD9Field
{
	private BlockCheckCharacter(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static BlockCheckCharacter FromEnvelopeBytes(ReadOnlySpan<byte> envelopeBytes)
	{
		var value = (byte)0;

		foreach (var envelopeByte in envelopeBytes)
		{
			value ^= envelopeByte;
		}

		return new BlockCheckCharacter(value);
	}

	public static BlockCheckCharacter FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new BlockCheckCharacter((byte)buffer.ReadUnsignedBits(8));
	}

	public byte[] ToWireValue()
	{
		return [this.Value];
	}
}
