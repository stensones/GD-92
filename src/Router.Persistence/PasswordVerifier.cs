using System.Security.Cryptography;
using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class PasswordVerifier
{
	private readonly PasswordVerifierSalt salt;
	private readonly PasswordVerifierHash hash;

	private PasswordVerifier(
		PasswordVerifierWorkFactor workFactor,
		PasswordVerifierSalt salt,
		PasswordVerifierHash hash)
	{
		this.WorkFactor = workFactor;
		this.salt = salt;
		this.hash = hash;
	}

	public PasswordVerifierWorkFactor WorkFactor { get; }

	public static PasswordVerifier Create(
		PasswordValue password,
		PasswordVerifierWorkFactor workFactor)
	{
		ArgumentNullException.ThrowIfNull(workFactor);
		PasswordVerifierWorkFactor.EnsureValid(workFactor.Iterations, nameof(workFactor));

		var salt = PasswordVerifierSalt.FromDatabaseValue(
			RandomNumberGenerator.GetBytes(PasswordVerifierSalt.Length));
		var hash = DeriveHash(password, salt, workFactor);

		return new PasswordVerifier(workFactor, salt, hash);
	}

	public static PasswordVerifier FromStoredData(PasswordVerifierData storedData)
	{
		ArgumentNullException.ThrowIfNull(storedData);

		return new PasswordVerifier(
			storedData.WorkFactor,
			storedData.Salt,
			storedData.Hash);
	}

	public bool Verifies(PasswordValue password)
	{
		var suppliedHash = DeriveHash(password, this.salt, this.WorkFactor);

		return CryptographicOperations.FixedTimeEquals(
			this.hash.ToDatabaseValue(),
			suppliedHash.ToDatabaseValue());
	}

	public PasswordVerifierData ToStoredData()
	{
		return PasswordVerifierData.Create(
			PasswordVerifierVersion.Current,
			this.WorkFactor,
			this.salt,
			this.hash);
	}

	private static PasswordVerifierHash DeriveHash(
		PasswordValue password,
		PasswordVerifierSalt salt,
		PasswordVerifierWorkFactor workFactor)
	{
		return PasswordVerifierHash.FromDatabaseValue(
			Rfc2898DeriveBytes.Pbkdf2(
				password.Value.ToWireValue(),
				salt.ToDatabaseValue(),
				workFactor.Iterations,
				HashAlgorithmName.SHA256,
				PasswordVerifierHash.Length));
	}
}
