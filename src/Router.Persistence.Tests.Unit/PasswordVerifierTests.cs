using AwesomeAssertions;
using Router.Persistence;
using Stensones.GD92.Fields;

namespace Router.Persistence.Tests.Unit;

public sealed class PasswordVerifierTests
{
	[Fact]
	public void Verifies_the_password_used_to_create_it()
	{
		var password = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var workFactor = PasswordVerifierWorkFactor.Default;

		var verifier = PasswordVerifier.Create(password, workFactor);

		verifier.Verifies(password).Should().BeTrue();
	}

	[Fact]
	public void Does_not_verify_a_different_password()
	{
		var verifier = PasswordVerifier.Create(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
			PasswordVerifierWorkFactor.Default);
		var differentPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("RESCUE"));

		verifier.Verifies(differentPassword).Should().BeFalse();
	}

	[Fact]
	public void Creates_independent_verifier_state_for_the_same_password()
	{
		var password = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var workFactor = PasswordVerifierWorkFactor.Default;

		var firstVerifier = PasswordVerifier.Create(password, workFactor);
		var secondVerifier = PasswordVerifier.Create(password, workFactor);

		firstVerifier.ToStoredData().Salt.ToArray().Should().NotBeEquivalentTo(
			secondVerifier.ToStoredData().Salt.ToArray());
	}

	[Fact]
	public void Restores_a_versioned_verifier_from_stored_data()
	{
		var password = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var workFactor = PasswordVerifierWorkFactor.Default;
		var storedData = PasswordVerifier.Create(password, workFactor).ToStoredData();

		var restoredVerifier = PasswordVerifier.FromStoredData(storedData);

		restoredVerifier.Verifies(password).Should().BeTrue();
		storedData.Version.Should().Be(PasswordVerifierVersion.Current);
		storedData.WorkFactor.Should().Be(workFactor);
	}

	[Fact]
	public void Keeps_stored_salt_and_hash_immutable()
	{
		var password = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var storedData = PasswordVerifier.Create(
			password,
			PasswordVerifierWorkFactor.Default).ToStoredData();

		var salt = storedData.Salt.ToArray();
		var hash = storedData.Hash.ToArray();
		salt[0] ^= 0xFF;
		hash[0] ^= 0xFF;

		PasswordVerifier.FromStoredData(storedData).Verifies(password).Should().BeTrue();
	}
}
