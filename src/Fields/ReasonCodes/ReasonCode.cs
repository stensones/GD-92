namespace Stensones.GD92.Fields;

public sealed record ReasonCode : IGD9Field
{
	private const byte GeneralReasonCodeSet = 1;
	private const byte ParameterReasonCodeSet = 4;
	private const int WordBitCount = 8;

	private ReasonCode(byte set, byte value)
	{
		this.Set = set;
		this.Value = value;
	}

	public byte Set { get; }
	public byte Value { get; }
	public GeneralReasonCode? GeneralReasonCode => this.Set == GeneralReasonCodeSet
		? (GeneralReasonCode)this.Value
		: null;
	public ParameterReasonCode? ParameterReasonCode => this.Set == ParameterReasonCodeSet
		? (ParameterReasonCode)this.Value
		: null;

	public static ReasonCode FromGeneralReasonCode(GeneralReasonCode generalReasonCode)
	{
		if (!Enum.IsDefined(generalReasonCode))
		{
			throw new ArgumentOutOfRangeException(nameof(generalReasonCode));
		}

		return new ReasonCode(GeneralReasonCodeSet, (byte)generalReasonCode);
	}

	public static ReasonCode FromParameterReasonCode(ParameterReasonCode parameterReasonCode)
	{
		if (!Enum.IsDefined(parameterReasonCode))
		{
			throw new ArgumentOutOfRangeException(nameof(parameterReasonCode));
		}

		return new ReasonCode(ParameterReasonCodeSet, (byte)parameterReasonCode);
	}

	public static ReasonCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var reasonCodeSet = (byte)buffer.ReadUnsignedBits(WordBitCount);
		var reasonCodeValue = (byte)buffer.ReadUnsignedBits(WordBitCount);

		return reasonCodeSet switch
		{
			GeneralReasonCodeSet => FromGeneralReasonCode((GeneralReasonCode)reasonCodeValue),
			ParameterReasonCodeSet => FromParameterReasonCode((ParameterReasonCode)reasonCodeValue),
			_ => throw new NotSupportedException($"Reason Code Set {reasonCodeSet} is not supported.")
		};
	}

	public byte[] ToWireValue()
	{
		return [this.Set, this.Value];
	}
}
