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

	public byte[] ToWireValue()
	{
		return [0x01, (byte)this.GeneralReasonCode];
	}
}
