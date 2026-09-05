namespace Router.Persistence;

public sealed class PasswordVerifierData
{
	private PasswordVerifierData(
		PasswordVerifierVersion version,
		PasswordVerifierWorkFactor workFactor,
		PasswordVerifierSalt salt,
		PasswordVerifierHash hash)
	{
		this.Version = version;
		this.WorkFactor = workFactor;
		this.Salt = salt;
		this.Hash = hash;
	}

	public PasswordVerifierVersion Version { get; }
	public PasswordVerifierWorkFactor WorkFactor { get; }
	public PasswordVerifierSalt Salt { get; }
	public PasswordVerifierHash Hash { get; }

	public static PasswordVerifierData Create(
		PasswordVerifierVersion version,
		PasswordVerifierWorkFactor workFactor,
		PasswordVerifierSalt salt,
		PasswordVerifierHash hash)
	{
		ArgumentNullException.ThrowIfNull(version);
		ArgumentNullException.ThrowIfNull(workFactor);
		ArgumentNullException.ThrowIfNull(salt);
		ArgumentNullException.ThrowIfNull(hash);
		PasswordVerifierWorkFactor.EnsureValid(workFactor.Iterations, nameof(workFactor));

		return new PasswordVerifierData(version, workFactor, salt, hash);
	}
}
