namespace Stensones.GD92.Fields;

public sealed record ReasonCode : IGD9Field
{
	private ReasonCode(GeneralReasonCode generalReasonCode)
	{
		this.GeneralReasonCode = generalReasonCode;
	}

	public GeneralReasonCode GeneralReasonCode { get; }

	public static ReasonCode FromGeneralReasonCode(GeneralReasonCode generalReasonCode)
	{
		if (!Enum.IsDefined(generalReasonCode))
		{
			throw new ArgumentOutOfRangeException(nameof(generalReasonCode));
		}

		return new ReasonCode(generalReasonCode);
	}

	public static ReasonCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var reasonCodeSet = (byte)buffer.ReadUnsignedBits(8);

		if (reasonCodeSet != 1)
		{
			throw new NotSupportedException($"Reason Code Set {reasonCodeSet} is not supported.");
		}

		return FromGeneralReasonCode((GeneralReasonCode)buffer.ReadUnsignedBits(8));
	}

	public byte[] ToWireValue()
	{
		return [0x01, (byte)this.GeneralReasonCode];
	}
}
