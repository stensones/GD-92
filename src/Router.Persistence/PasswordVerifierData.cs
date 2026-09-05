namespace Router.Persistence;

public sealed class PasswordVerifierData
{
	internal const int SaltLength = 16;
	internal const int HashLength = 32;

	private readonly byte[] salt;
	private readonly byte[] hash;

	private PasswordVerifierData(
		PasswordVerifierVersion version,
		PasswordVerifierWorkFactor workFactor,
		byte[] salt,
		byte[] hash)
	{
		this.Version = version;
		this.WorkFactor = workFactor;
		this.salt = [.. salt];
		this.hash = [.. hash];
	}

	public PasswordVerifierVersion Version { get; }
	public PasswordVerifierWorkFactor WorkFactor { get; }
	public byte[] Salt => [.. this.salt];
	public byte[] Hash => [.. this.hash];

	public static PasswordVerifierData FromStoredValues(
		PasswordVerifierVersion version,
		PasswordVerifierWorkFactor workFactor,
		byte[] salt,
		byte[] hash)
	{
		ArgumentNullException.ThrowIfNull(salt);
		ArgumentNullException.ThrowIfNull(hash);
		PasswordVerifierVersion.EnsureSupported(version, nameof(version));
		PasswordVerifierWorkFactor.EnsureValid(workFactor.Iterations, nameof(workFactor));

		if (salt.Length != SaltLength)
		{
			throw new ArgumentException("The salt has an invalid length.", nameof(salt));
		}

		if (hash.Length != HashLength)
		{
			throw new ArgumentException("The hash has an invalid length.", nameof(hash));
		}

		return new PasswordVerifierData(version, workFactor, salt, hash);
	}
}
