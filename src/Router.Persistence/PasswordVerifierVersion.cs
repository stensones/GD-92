namespace Router.Persistence;

public readonly record struct PasswordVerifierVersion
{
	private PasswordVerifierVersion(int value)
	{
		this.Value = value;
	}

	public static PasswordVerifierVersion Current { get; } = new(1);

	public int Value { get; }

	public static PasswordVerifierVersion FromValue(int value)
	{
		var version = new PasswordVerifierVersion(value);
		EnsureSupported(version, nameof(value));

		return version;
	}

	internal static void EnsureSupported(PasswordVerifierVersion version, string parameterName)
	{
		if (version != Current)
		{
			throw new ArgumentOutOfRangeException(parameterName);
		}
	}
}
