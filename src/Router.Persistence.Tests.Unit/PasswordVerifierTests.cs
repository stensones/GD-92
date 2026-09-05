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
		var workFactor = PasswordVerifierWorkFactor.FromIterations(10_000);

		var verifier = PasswordVerifier.Create(password, workFactor);

		verifier.Verifies(password).Should().BeTrue();
	}

	[Fact]
	public void Does_not_verify_a_different_password()
	{
		var verifier = PasswordVerifier.Create(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
			PasswordVerifierWorkFactor.FromIterations(10_000));
		var differentPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("RESCUE"));

		verifier.Verifies(differentPassword).Should().BeFalse();
	}

	[Fact]
	public void Creates_independent_verifier_state_for_the_same_password()
	{
		var password = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var workFactor = PasswordVerifierWorkFactor.FromIterations(10_000);

		var firstVerifier = PasswordVerifier.Create(password, workFactor);
		var secondVerifier = PasswordVerifier.Create(password, workFactor);

		firstVerifier.ToStoredData().Salt.Should().NotBeEquivalentTo(
			secondVerifier.ToStoredData().Salt);
	}

	[Fact]
	public void Restores_a_versioned_verifier_from_stored_data()
	{
		var password = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var workFactor = PasswordVerifierWorkFactor.FromIterations(10_000);
		var storedData = PasswordVerifier.Create(password, workFactor).ToStoredData();

		var restoredVerifier = PasswordVerifier.FromStoredData(storedData);

		restoredVerifier.Verifies(password).Should().BeTrue();
		storedData.Version.Should().Be(PasswordVerifierVersion.Current);
		storedData.WorkFactor.Should().Be(workFactor);
	}

	[Fact]
	public void Rejects_stored_data_with_an_invalid_salt_length()
	{
		var createStoredData = () => PasswordVerifierData.FromStoredValues(
			PasswordVerifierVersion.Current,
			PasswordVerifierWorkFactor.FromIterations(10_000),
			new byte[15],
			new byte[32]);

		createStoredData.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Rejects_an_unsupported_stored_verifier_version()
	{
		var createVersion = () => PasswordVerifierVersion.FromValue(2);

		createVersion.Should().Throw<ArgumentOutOfRangeException>();
	}
}
