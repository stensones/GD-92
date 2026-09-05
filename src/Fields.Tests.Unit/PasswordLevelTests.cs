using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PasswordLevelTests
{
	[Fact]
	public void Serializes_the_unauthenticated_password_level()
	{
		var value = PasswordLevelNumber.FromValue(0);

		var passwordLevel = PasswordLevel.FromValue(value);

		passwordLevel.Value.Should().Be(value);
		passwordLevel.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Serializes_the_highest_password_level()
	{
		var passwordLevel = PasswordLevel.FromValue(PasswordLevelNumber.FromValue(4));

		passwordLevel.ToWireValue().Should().Equal(new byte[] { 0x04 });
	}

	[Fact]
	public void Rejects_a_password_level_number_above_four()
	{
		var createPasswordLevel = () => PasswordLevelNumber.FromValue(5);

		createPasswordLevel.Should().Throw<ArgumentOutOfRangeException>();
	}
}
