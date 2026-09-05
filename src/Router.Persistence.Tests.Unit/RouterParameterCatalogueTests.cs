using AwesomeAssertions;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence.Tests.Unit;

public sealed class RouterParameterCatalogueTests
{
	[Fact]
	public void Reads_the_brigade_or_agency_identifier_from_its_canonical_Parameter_Value()
	{
		var parameterValue = ParameterValue.FromWireValue([42]);

		var brigadeOrAgencyIdentifier = RouterParameterCatalogue.BrigadeOrAgency.Read(
			parameterValue);

		brigadeOrAgencyIdentifier.Should().Be(BrigadeOrAgencyIdentifier.FromValue(42));
	}

	[Fact]
	public void Reads_the_Current_Password_from_its_canonical_Parameter_Value()
	{
		var localAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(42)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(0)));
		var expected = PasswordParameter.FromFields(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
			Password.FromValue(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
			localAddress);

		var currentPassword = RouterParameterCatalogue.CurrentPassword.Read(
			ParameterValue.FromWireValue(expected.ToWireValue()));

		currentPassword.Level.Should().Be(expected.Level);
		currentPassword.Password.ToWireValue().Should().Equal(expected.Password.ToWireValue());
		currentPassword.CommunicationsAddress.Should().Be(expected.CommunicationsAddress);
	}

	[Fact]
	public void Reads_the_No_Acknowledgement_Timeout_from_its_canonical_Parameter_Value()
	{
		var parameterValue = ParameterValue.FromWireValue([5]);

		var noAcknowledgementTimeout = RouterParameterCatalogue.NoAcknowledgementTimeout.Read(
			parameterValue);

		noAcknowledgementTimeout.Should().Be(
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)));
	}

	[Fact]
	public void Reads_Retries_from_its_canonical_Parameter_Value()
	{
		var parameterValue = ParameterValue.FromWireValue([3]);

		var retries = RouterParameterCatalogue.Retries.Read(parameterValue);

		retries.Should().Be(Retries.FromValue(Word8.FromValue(3)));
	}
}
