using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class RouteStatusTests
{
	[Fact]
	public void Serializes_route_enablement_and_destination_nodes()
	{
		var routeStatus = RouteStatus.FromFields(
			ProtocolBoolean.True,
			DestinationNodes.FromAddressRanges(AddressRange.FromValues(
				CreateAddress(26, 101, 25),
				CreateAddress(26, 102, 25))));

		routeStatus.ToWireValue().Should().Equal(Convert.FromHexString("01011A19591A1999"));
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
