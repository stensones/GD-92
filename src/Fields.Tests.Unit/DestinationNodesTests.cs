using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class DestinationNodesTests
{
	[Fact]
	public void Serializes_each_address_range_after_its_count()
	{
		var firstAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(25)));
		var lastAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(101)),
			Port.FromValue(PortIdentifier.FromValue(25)));

		DestinationNodes.FromAddressRanges(AddressRange.FromValues(firstAddress, lastAddress))
			.ToWireValue()
			.Should()
			.Equal(Convert.FromHexString("011A19191A1959"));
	}

	[Fact]
	public void Decodes_each_address_range_after_its_count()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("011A19191A1959"));

		var destinationNodes = DestinationNodes.FromEncodedMessageBuffer(ref buffer);

		destinationNodes.AddressRanges.Should().ContainSingle();
		destinationNodes.AddressRanges[0].FirstAddress.Node.Value.Should().Be(100);
		destinationNodes.AddressRanges[0].LastAddress.Node.Value.Should().Be(101);
		buffer.RemainingBitCount.Should().Be(0);
	}
}
