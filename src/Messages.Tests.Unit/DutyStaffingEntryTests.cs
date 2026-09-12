using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class DutyStaffingEntryTests
{
	[Fact]
	public void Serializes_callsign_officer_in_charge_riders_status_and_remarks()
	{
		var entry = DutyStaffingEntry.FromFields(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			OfficerInCharge.FromValue(SevenBitAsciiString.FromValue("JDOE")),
			Riders.FromValue(4),
			StatusCode.FromValue(StatusCodeValue.AvailableAtBase),
			Remarks.FromValue(SevenBitAsciiString.FromValue("READY")));

		entry.ToWireValue().Should().Equal(Convert.FromHexString("024131044A444F450404055245414459"));
	}
}
