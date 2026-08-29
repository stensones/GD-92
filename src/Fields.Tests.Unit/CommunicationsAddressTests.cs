using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class CommunicationsAddressTests
{
	[Fact]
	public void Serializes_brigade_node_and_port_in_their_protocol_bit_ranges()
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(26),
			Node.FromValue(100),
			Port.FromValue(25));

		address.ToWireValue().Should().Equal(new byte[] { 0x1A, 0x19, 0x19 });
	}
}
