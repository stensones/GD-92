using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class SetParameterTests
{
	[Fact]
	public void Serializes_the_current_password_parameter_for_a_User_Agent()
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(25)));
		var passwordParameter = PasswordParameter.FromFields(
			PasswordLevel.FromValue(PasswordLevelNumber.FromValue(1)),
			Password.FromValue(PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"))),
			address);
		var setParameter = SetParameter.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(4),
			ParameterValue.FromWireValue(passwordParameter.ToWireValue()));

		setParameter.Type.ToWireValue().Should().Equal(new byte[] { 0x3C });
		setParameter.ToWireValue().Should().Equal(Convert.FromHexString("02040104464952451A1919"));
	}

	[Fact]
	public void Decodes_a_scalar_level_one_password_parameter_without_an_index()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("02050446495245"));

		var setParameter = SetParameter.FromEncodedMessageBuffer(ref buffer);

		setParameter.ParameterTable.Should().Be(ParameterTable.Current);
		setParameter.ParameterNumber.Should().Be(ParameterNumber.FromValue(5));
		setParameter.ParameterValue.ToWireValue().Should().Equal(Convert.FromHexString("0446495245"));
		buffer.BitPosition.Should().Be(56);
	}
}
