using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class IncidentAddressTests
{
	[Fact]
	public void Serializes_the_address_components_in_specification_order()
	{
		var address = IncidentAddress.FromValues(
			SevenBitAsciiString.FromValue("STATION"),
			SevenBitAsciiString.FromValue("1"),
			SevenBitAsciiString.FromValue("HIGH STREET"),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty));

		address.ToWireValue().Should()
			.Equal(Convert.FromHexString("0753544154494F4E01310B48494748205354524545540000000000"));
	}

	[Fact]
	public void Compresses_the_address_text_component()
	{
		var address = IncidentAddress.FromValues(
			SevenBitAsciiString.FromValue("AAAAA"),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty),
			SevenBitAsciiString.FromValue(string.Empty));

		address.ToWireValue().Should().Equal(Convert.FromHexString("031B410500000000000000"));
	}
}
