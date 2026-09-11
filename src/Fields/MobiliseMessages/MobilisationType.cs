namespace Stensones.GD92.Fields;

public sealed record MobilisationType : IGD9Field
{
	private const int WordBitCount = 8;

	private MobilisationType(MobilisationTypeValue value)
	{
		this.Value = value;
	}

	public MobilisationTypeValue Value { get; }

	public static MobilisationType FromValue(MobilisationTypeValue value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new MobilisationType(value);
	}

	public static MobilisationType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((MobilisationTypeValue)buffer.ReadUnsignedBits(WordBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)this.Value];
	}
}
