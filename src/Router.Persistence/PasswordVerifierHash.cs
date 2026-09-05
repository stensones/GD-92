namespace Router.Persistence;

public sealed class PasswordVerifierHash
{
	internal const int Length = 32;

	private readonly byte[] value;

	private PasswordVerifierHash(byte[] value)
	{
		this.value = [.. value];
	}

	internal static PasswordVerifierHash FromDatabaseValue(byte[] value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (value.Length != Length)
		{
			throw new ArgumentException("The hash has an invalid length.", nameof(value));
		}

		return new PasswordVerifierHash(value);
	}

	internal byte[] ToDatabaseValue()
	{
		return [.. this.value];
	}

	public byte[] ToArray()
	{
		return [.. this.value];
	}
}
