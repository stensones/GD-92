namespace Stensones.GD92.Fields;

public sealed record TimeAndDate : IGD9Field
{
	private static readonly HashSet<string> Months =
	[
		"JAN", "FEB", "MAR", "APR", "MAY", "JUN",
		"JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
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
		var value = new byte[13];

		for (var index = 0; index < value.Length; index++)
		{
			value[index] = (byte)buffer.ReadUnsignedBits(8);
		}

		return FromValue(SevenBitAsciiString.FromValue(new string(value.Select(character => (char)character).ToArray())));
	}

	public byte[] ToWireValue()
	{
		return this.Value.ToWireValue();
	}

	private static bool HasValidFormat(string value)
	{
		return value.Length == 13
			&& HasValueInRange(value, 0, 2, 1, 31)
			&& Months.Contains(value[2..5])
			&& HasDigits(value, 5, 2)
			&& HasValueInRange(value, 7, 2, 0, 23)
			&& HasValueInRange(value, 9, 2, 0, 59)
			&& HasValueInRange(value, 11, 2, 0, 59);
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
