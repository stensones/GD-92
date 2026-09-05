using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PasswordParameterTests
{
	[Fact]
	public void Decodes_a_password_level_password_and_Communications_Address()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("0104464952451A1919"));

		var passwordParameter = PasswordParameter.FromEncodedMessageBuffer(ref buffer);

		passwordParameter.ToWireValue().Should().Equal(Convert.FromHexString("0104464952451A1919"));
		buffer.BitPosition.Should().Be(72);
	}
}
