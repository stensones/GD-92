namespace Router.Persistence;

public readonly record struct PasswordVerifierWorkFactor
{
	internal const int MinimumIterations = 10_000;

	private PasswordVerifierWorkFactor(int iterations)
	{
		this.Iterations = iterations;
	}

	public int Iterations { get; }

	public static PasswordVerifierWorkFactor FromIterations(int iterations)
	{
		EnsureValid(iterations, nameof(iterations));

		return new PasswordVerifierWorkFactor(iterations);
	}

	internal static void EnsureValid(int iterations, string parameterName)
	{
		if (iterations < MinimumIterations)
		{
			throw new ArgumentOutOfRangeException(parameterName);
		}
	}
}
