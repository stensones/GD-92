namespace Stensones.GD92.Fields;

public sealed record ReasonCode : IGD9Field
{
	private const byte GeneralReasonCodeSet = 1;
	private const byte ParameterReasonCodeSet = 4;
	private const int WordBitCount = 8;
	private const string UnsupportedReasonCodeSetMessageFormat = "Reason Code Set {0} is not supported.";

	private ReasonCode(GeneralReasonCode generalReasonCode)
	{
		this.GeneralReasonCode = generalReasonCode;
	}

	private ReasonCode(ParameterReasonCode parameterReasonCode)
	{
		this.ParameterReasonCode = parameterReasonCode;
	}

	public GeneralReasonCode? GeneralReasonCode { get; }
	public ParameterReasonCode? ParameterReasonCode { get; }

	public static ReasonCode FromGeneralReasonCode(GeneralReasonCode generalReasonCode)
	{
		if (!Enum.IsDefined(generalReasonCode))
		{
			throw new ArgumentOutOfRangeException(nameof(generalReasonCode));
		}

		return new ReasonCode(generalReasonCode);
	}

	public static ReasonCode FromParameterReasonCode(ParameterReasonCode parameterReasonCode)
	{
		if (!Enum.IsDefined(parameterReasonCode))
		{
			throw new ArgumentOutOfRangeException(nameof(parameterReasonCode));
		}

		return new ReasonCode(parameterReasonCode);
	}

	public static ReasonCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var reasonCodeSet = (byte)buffer.ReadUnsignedBits(WordBitCount);
		var reasonCodeValue = (byte)buffer.ReadUnsignedBits(WordBitCount);

		return reasonCodeSet switch
		{
			GeneralReasonCodeSet => FromGeneralReasonCode((GeneralReasonCode)reasonCodeValue),
			ParameterReasonCodeSet => FromParameterReasonCode((ParameterReasonCode)reasonCodeValue),
			_ => throw new NotSupportedException(string.Format(UnsupportedReasonCodeSetMessageFormat, reasonCodeSet))
		};
	}

	public byte[] ToWireValue()
	{
		if (this.GeneralReasonCode is { } generalReasonCode)
		{
			return [GeneralReasonCodeSet, (byte)generalReasonCode];
		}

		return [ParameterReasonCodeSet, (byte)this.ParameterReasonCode!.Value];
	}
}
