using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class MakeUpTests
{
	[Fact]
	public void Serializes_callsign_incident_number_appliance_count_and_appliances()
	{
		var makeUp = MakeUp.FromFields(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			IncidentNumber.FromValue(123456),
			MakeUpAppliance.FromFields(
				ApplianceType.FromValue(SevenBitAsciiString.FromValue("PMP")),
				ApplianceQuantity.FromValue(2)));

		makeUp.ToWireValue().Should().Equal(Convert.FromHexString("0241310001E24001504D5002"));
	}
}
