using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class StopTests
{
	[Fact]
	public void Serializes_callsign_incident_number_and_stop_code()
	{
		var stop = Stop.FromFields(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			IncidentNumber.FromValue(123456),
			StopCode.FromValue(SevenBitAsciiString.FromValue("STP01")));

		stop.ToWireValue().Should().Equal(Convert.FromHexString("0241310001E2405354503031"));
	}
}
