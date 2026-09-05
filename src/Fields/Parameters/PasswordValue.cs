namespace Stensones.GD92.Fields;

public readonly record struct PasswordValue
{
	private PasswordValue(SevenBitAsciiString value)
	{
		this.Value = value;
	}

	public SevenBitAsciiString Value { get; }

	public static PasswordValue FromValue(SevenBitAsciiString value)
	{
		if (value.Value.Length > 10)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new PasswordValue(value);
	}
}
