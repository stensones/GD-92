namespace Stensones.GD92.Fields;

public sealed record ReasonCode : IGD9Field
{
	private const byte GeneralReasonCodeSet = 1;
	private const byte PrinterReasonCodeSet = 3;
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

	private ReasonCode(PrinterReasonCode printerReasonCode)
	{
		this.PrinterReasonCode = printerReasonCode;
	}

	public GeneralReasonCode? GeneralReasonCode { get; }
	public ParameterReasonCode? ParameterReasonCode { get; }
	public PrinterReasonCode? PrinterReasonCode { get; }

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

	public static ReasonCode FromPrinterReasonCode(PrinterReasonCode printerReasonCode)
	{
		if (!Enum.IsDefined(printerReasonCode))
		{
			throw new ArgumentOutOfRangeException(nameof(printerReasonCode));
		}

		return new ReasonCode(printerReasonCode);
	}

	public static ReasonCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var reasonCodeSet = (byte)buffer.ReadUnsignedBits(WordBitCount);
		var reasonCodeValue = (byte)buffer.ReadUnsignedBits(WordBitCount);

		return reasonCodeSet switch
		{
			GeneralReasonCodeSet => FromGeneralReasonCode((GeneralReasonCode)reasonCodeValue),
			PrinterReasonCodeSet => FromPrinterReasonCode((PrinterReasonCode)reasonCodeValue),
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

		if (this.PrinterReasonCode is { } printerReasonCode)
		{
			return [PrinterReasonCodeSet, (byte)printerReasonCode];
		}

		return [ParameterReasonCodeSet, (byte)this.ParameterReasonCode!.Value];
	}
}
