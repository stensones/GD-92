using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ResourceStatusEntryTests
{
	[Fact]
	public void Serializes_callsign_avl_status_and_remarks()
	{
		var entry = ResourceStatusEntry.FromFields(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			AvlType.FromValue(AvlTypeValue.NoAvlDataSystemPresent),
			AvlData.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			StatusCode.FromValue(StatusCodeValue.AvailableAtBase),
			Remarks.FromValue(SevenBitAsciiString.FromValue("READY")));

		entry.ToWireValue().Should().Equal(Convert.FromHexString("024131000004055245414459"));
	}
}
