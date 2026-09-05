namespace Router.Persistence;

public sealed record PasswordVerifierWorkFactor
{
	internal const int MinimumIterations = 10_000;

	private PasswordVerifierWorkFactor(int iterations)
	{
		this.Iterations = iterations;
	}

	internal int Iterations { get; }

	public static PasswordVerifierWorkFactor Default { get; } = new(MinimumIterations);

	internal static PasswordVerifierWorkFactor FromDatabaseValue(int iterations)
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

	internal int ToDatabaseValue()
	{
		return this.Iterations;
	}
}
