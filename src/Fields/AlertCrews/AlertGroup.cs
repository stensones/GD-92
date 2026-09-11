namespace Stensones.GD92.Fields;

public sealed record AlertGroup : IGD9Field
{
	private const int AlertGroupBitCount = 16;
	private const int ByteBitCount = 8;

	private AlertGroup(AlertGroupValue value)
	{
		this.Value = value;
	}

	public AlertGroupValue Value { get; }

	public static AlertGroup FromValue(AlertGroupValue value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new AlertGroup(value);
	}

	public static AlertGroup FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((AlertGroupValue)buffer.ReadUnsignedBits(AlertGroupBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)((ushort)this.Value >> ByteBitCount), (byte)this.Value];
	}
}
