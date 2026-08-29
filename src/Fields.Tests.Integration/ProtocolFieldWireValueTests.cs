using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Integration;

public sealed class ProtocolFieldWireValueTests
{
	[Fact]
	public void Valid_wire_value_can_be_serialized_unchanged()
	{
		var wireValue = new byte[] { 0x47, 0x44, 0x39, 0x32 };

		var protocolField = ProtocolField.FromWireValue(wireValue);

		protocolField.ToWireValue().Should().Equal(wireValue);
	}
}
