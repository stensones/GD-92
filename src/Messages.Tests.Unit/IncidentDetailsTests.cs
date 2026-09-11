using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class IncidentDetailsTests
{
	[Fact]
	public void Serializes_incident_details_in_mobilise_message_order()
	{
		var incidentDetails = IncidentDetails.FromFields(
			IncidentNumber.FromValue(1),
			MobilisationType.FromValue(MobilisationTypeValue.Incident),
			IncidentAddress.FromValues(
				AddressText.FromValue(SevenBitAsciiString.FromValue("STATION")),
				HouseNumber.FromValue(SevenBitAsciiString.FromValue("1")),
				Street.FromValue(SevenBitAsciiString.FromValue("HIGH STREET")),
				SubDistrict.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				District.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				Town.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				County.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				Postcode.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
			MapReference.FromValue(SevenBitAsciiString.FromValue("SU123456")),
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123")),
			Stensones.GD92.Fields.Text.FromValue("FIRE"));

		incidentDetails.ToWireValue().Should().Equal(Convert.FromHexString(
			"00000001010753544154494F4E01310B484947482053545245455400000000000853553132333435360430313233000446495245"));
	}
}
