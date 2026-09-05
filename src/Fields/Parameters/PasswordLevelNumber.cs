namespace Stensones.GD92.Fields;

public readonly record struct PasswordLevelNumber
{
	private PasswordLevelNumber(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static PasswordLevelNumber FromValue(byte value)
	{
		if (value > 4)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new PasswordLevelNumber(value);
	}
}
