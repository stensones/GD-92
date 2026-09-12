namespace Stensones.GD92.Fields;

public sealed record TimeAndDate : IGD9Field
{
	private const int WireByteCount = 13;
	private const int CharacterBitCount = 8;
	private const int DayStartIndex = 0;
	private const int DayLength = 2;
	private const int MinimumDay = 1;
	private const int MaximumDay = 31;
	private const int MonthStartIndex = DayStartIndex + DayLength;
	private const int MonthEndIndex = MonthStartIndex + 3;
	private const int YearStartIndex = MonthEndIndex;
	private const int YearLength = 2;
	private const int HourStartIndex = YearStartIndex + YearLength;
	private const int HourLength = 2;
	private const int MinimumTimeComponent = 0;
	private const int MaximumHour = 23;
	private const int MinuteStartIndex = HourStartIndex + HourLength;
	private const int MinuteLength = 2;
	private const int MaximumMinute = 59;
	private const int SecondStartIndex = MinuteStartIndex + MinuteLength;
	private const int SecondLength = 2;
	private const int MaximumSecond = 59;
	private const string JanuaryToken = "JAN";
	private const string FebruaryToken = "FEB";
	private const string MarchToken = "MAR";
	private const string AprilToken = "APR";
	private const string MayToken = "MAY";
	private const string JuneToken = "JUN";
	private const string JulyToken = "JUL";
	private const string AugustToken = "AUG";
	private const string SeptemberToken = "SEP";
	private const string OctoberToken = "OCT";
	private const string NovemberToken = "NOV";
	private const string DecemberToken = "DEC";

	private static readonly HashSet<string> Months =
	[
		JanuaryToken, FebruaryToken, MarchToken, AprilToken, MayToken, JuneToken,
		JulyToken, AugustToken, SeptemberToken, OctoberToken, NovemberToken, DecemberToken
	];

	private TimeAndDate(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static TimeAndDate FromValue(SevenBitAsciiString value)
	{
		if (!HasValidFormat(value.Value))
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new TimeAndDate(value);
	}

	public static TimeAndDate FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var value = new byte[WireByteCount];

		for (var index = 0; index < value.Length; index++)
		{
			value[index] = (byte)buffer.ReadUnsignedBits(CharacterBitCount);
		}

		return FromValue(SevenBitAsciiString.FromValue(new string(value.Select(character => (char)character).ToArray())));
	}

	public byte[] ToWireValue()
	{
		return this.Value.ToWireValue();
	}

	private static bool HasValidFormat(string value)
	{
		return value.Length == WireByteCount
			&& HasValueInRange(value, DayStartIndex, DayLength, MinimumDay, MaximumDay)
			&& Months.Contains(value[MonthStartIndex..MonthEndIndex])
			&& HasDigits(value, YearStartIndex, YearLength)
			&& HasValueInRange(value, HourStartIndex, HourLength, MinimumTimeComponent, MaximumHour)
			&& HasValueInRange(value, MinuteStartIndex, MinuteLength, MinimumTimeComponent, MaximumMinute)
			&& HasValueInRange(value, SecondStartIndex, SecondLength, MinimumTimeComponent, MaximumSecond);
	}

	private static bool HasValueInRange(string value, int startIndex, int length, int minimum, int maximum)
	{
		if (!HasDigits(value, startIndex, length))
		{
			return false;
		}

		var parsedValue = int.Parse(value.AsSpan(startIndex, length));

		return parsedValue >= minimum && parsedValue <= maximum;
	}

	private static bool HasDigits(string value, int startIndex, int length)
	{
		return value.AsSpan(startIndex, length).ToString().All(char.IsAsciiDigit);
	}
}
