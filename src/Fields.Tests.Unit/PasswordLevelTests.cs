using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PasswordLevelTests
{
	[Fact]
	public void Serializes_the_unauthenticated_password_level()
	{
		var value = PasswordLevelNumber.Unauthenticated;

		var passwordLevel = PasswordLevel.FromValue(value);

		passwordLevel.Value.Should().Be(value);
		passwordLevel.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Serializes_the_highest_password_level()
	{
		var passwordLevel = PasswordLevel.FromValue(PasswordLevelNumber.Level4);

		passwordLevel.ToWireValue().Should().Equal(new byte[] { 0x04 });
	}

	[Fact]
	public void Rejects_an_undefined_password_level_number()
	{
		var createPasswordLevel = () => PasswordLevel.FromValue((PasswordLevelNumber)5);

		createPasswordLevel.Should().Throw<ArgumentOutOfRangeException>();
	}
}
