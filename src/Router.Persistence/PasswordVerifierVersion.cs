namespace Router.Persistence;

public sealed record PasswordVerifierVersion
{
	private const int CurrentDatabaseValue = 1;

	private PasswordVerifierVersion()
	{
	}

	public static PasswordVerifierVersion Current { get; } = new();

	internal static PasswordVerifierVersion FromDatabaseValue(int value)
	{
		if (value != CurrentDatabaseValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return Current;
	}

	internal int ToDatabaseValue()
	{
		return CurrentDatabaseValue;
	}
}
