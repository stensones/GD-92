namespace Router.Persistence;

public sealed class PasswordVerifierSalt
{
	internal const int Length = 16;

	private readonly byte[] value;

	private PasswordVerifierSalt(byte[] value)
	{
		this.value = [.. value];
	}

	internal static PasswordVerifierSalt FromDatabaseValue(byte[] value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (value.Length != Length)
		{
			throw new ArgumentException("The salt has an invalid length.", nameof(value));
		}

		return new PasswordVerifierSalt(value);
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
