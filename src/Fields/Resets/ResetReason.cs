namespace Stensones.GD92.Fields;

public sealed record ResetReason : IGD9Field
{
	private const int ResetReasonBitCount = 8;

	private ResetReason(ResetReasonValue value)
	{
		this.Value = value;
	}

	public ResetReasonValue Value { get; }

	public static ResetReason FromValue(ResetReasonValue value)
	{
		if (!Enum.IsDefined(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ResetReason(value);
	}

	public static ResetReason FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((ResetReasonValue)buffer.ReadUnsignedBits(ResetReasonBitCount));
	}

	public byte[] ToWireValue()
	{
		return [(byte)this.Value];
	}
}
