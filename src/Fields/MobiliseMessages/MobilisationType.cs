namespace Stensones.GD92.Fields;

public sealed record MobilisationType : IGD9Field
{
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
		return FromValue((MobilisationTypeValue)buffer.ReadUnsignedBits(8));
	}

	public byte[] ToWireValue()
	{
		return [(byte)this.Value];
	}
}
