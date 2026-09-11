using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MapReferenceTests
{
	[Fact]
	public void Serializes_a_counted_map_reference()
	{
		var mapReference = MapReference.FromValue(SevenBitAsciiString.FromValue("SU123456"));

		mapReference.ToWireValue().Should().Equal(Convert.FromHexString("085355313233343536"));
	}

	[Fact]
	public void Rejects_a_map_reference_longer_than_sixteen_characters()
	{
		var createMapReference = () => MapReference.FromValue(
			SevenBitAsciiString.FromValue("12345678901234567"));

		createMapReference.Should().Throw<ArgumentOutOfRangeException>();
	}
}
