using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class LogUpdateTests
{
	[Fact]
	public void Serializes_callsign_incident_number_and_update()
	{
		var logUpdate = LogUpdate.FromFields(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			IncidentNumber.FromValue(123456),
			Update.FromValue(SevenBitAsciiString.FromValue("ARRIVED")));

		logUpdate.ToWireValue().Should().Equal(Convert.FromHexString("0241310001E2400741525249564544"));
	}
}
