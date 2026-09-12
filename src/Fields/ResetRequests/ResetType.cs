namespace Stensones.GD92.Fields;

public sealed record ResetType : IGD9Field
{
	private const int ResetTypeBitCount = 8;

	private ResetType(ResetTypeValue value)
	{
		this.Value = value;
	}

	public ResetTypeValue Value { get; }

	public static ResetType FromValue(ResetTypeValue value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ResetType(value);
	}

	public static ResetType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((ResetTypeValue)buffer.ReadUnsignedBits(ResetTypeBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)this.Value];
	}
}
