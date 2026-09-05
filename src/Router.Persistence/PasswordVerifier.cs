using System.Security.Cryptography;
using Stensones.GD92.Fields;

namespace Router.Persistence;

public sealed class PasswordVerifier
{
	private readonly byte[] salt;
	private readonly byte[] hash;

	private PasswordVerifier(
		PasswordVerifierWorkFactor workFactor,
		byte[] salt,
		byte[] hash)
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
		PasswordVerifierWorkFactor.EnsureValid(workFactor.Iterations, nameof(workFactor));

		var salt = RandomNumberGenerator.GetBytes(PasswordVerifierData.SaltLength);
		var hash = DeriveHash(password, salt, workFactor);

		return new PasswordVerifier(workFactor, salt, hash);
	}

	public static PasswordVerifier FromStoredData(PasswordVerifierData storedData)
	{
		ArgumentNullException.ThrowIfNull(storedData);

		PasswordVerifierVersion.EnsureSupported(storedData.Version, nameof(storedData));

		return new PasswordVerifier(
			storedData.WorkFactor,
			storedData.Salt,
			storedData.Hash);
	}

	public bool Verifies(PasswordValue password)
	{
		var suppliedHash = DeriveHash(password, this.salt, this.WorkFactor);

		return CryptographicOperations.FixedTimeEquals(this.hash, suppliedHash);
	}

	public PasswordVerifierData ToStoredData()
	{
		return PasswordVerifierData.FromStoredValues(
			PasswordVerifierVersion.Current,
			this.WorkFactor,
			this.salt,
			this.hash);
	}

	private static byte[] DeriveHash(
		PasswordValue password,
		byte[] salt,
		PasswordVerifierWorkFactor workFactor)
	{
		return Rfc2898DeriveBytes.Pbkdf2(
			password.Value.ToWireValue(),
			salt,
			workFactor.Iterations,
			HashAlgorithmName.SHA256,
			PasswordVerifierData.HashLength);
	}
}
