using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class IncidentAddressTests
{
	[Fact]
	public void Requires_typed_address_components()
	{
		var parameterTypes = typeof(IncidentAddress)
			.GetMethod(nameof(IncidentAddress.FromValues))!
			.GetParameters()
			.Select(parameter => parameter.ParameterType);

		parameterTypes.Should().Equal(
			typeof(AddressText),
			typeof(HouseNumber),
			typeof(Street),
			typeof(SubDistrict),
			typeof(District),
			typeof(Town),
			typeof(County),
			typeof(Postcode));
	}

	[Fact]
	public void Serializes_the_address_components_in_specification_order()
	{
		var address = IncidentAddress.FromValues(
			AddressText.FromValue(SevenBitAsciiString.FromValue("STATION")),
			HouseNumber.FromValue(SevenBitAsciiString.FromValue("1")),
			Street.FromValue(SevenBitAsciiString.FromValue("HIGH STREET")),
			SubDistrict.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			District.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			Town.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			County.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			Postcode.FromValue(SevenBitAsciiString.FromValue(string.Empty)));

		address.ToWireValue().Should()
			.Equal(Convert.FromHexString("0753544154494F4E01310B48494748205354524545540000000000"));
	}

	[Fact]
	public void Compresses_the_address_text_component()
	{
		var address = IncidentAddress.FromValues(
			AddressText.FromValue(SevenBitAsciiString.FromValue("AAAAA")),
			HouseNumber.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			Street.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			SubDistrict.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			District.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			Town.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			County.FromValue(SevenBitAsciiString.FromValue(string.Empty)),
			Postcode.FromValue(SevenBitAsciiString.FromValue(string.Empty)));

		address.ToWireValue().Should().Equal(Convert.FromHexString("031B410500000000000000"));
	}
}
