using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class IncidentNotificationTests
{
	[Fact]
	public void Serializes_the_incident_notification_fields_in_protocol_order()
	{
		var notification = IncidentNotification.FromFields(
			AlarmType.FromValue(SevenBitAsciiString.FromValue("FIRE")),
			CallAgency.FromValue(SevenBitAsciiString.FromValue("ACME")),
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123")),
			AlarmReference.FromValue(0x1234),
			AlarmSerial.FromValue(SevenBitAsciiString.FromValue("ABC123")),
			IncidentAddress.FromValues(
				AddressText.FromValue(SevenBitAsciiString.FromValue("STATION")),
				HouseNumber.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				Street.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				SubDistrict.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				District.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				Town.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				County.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
				Postcode.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
			Stensones.GD92.Fields.Text.FromValue("FIRE"));

		notification.ToWireValue().Should().Equal(Convert.FromHexString(
			"04464952450441434D4504303132331234064142433132330753544154494F4E00000000000000000446495245"));
	}
}
