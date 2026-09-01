using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class BrigadeTests
{
	[Fact]
	public void Preserves_a_brigade_or_agency_identifier()
	{
		var identifier = BrigadeOrAgencyIdentifier.FromValue(26);
		var brigade = Brigade.FromValue(identifier);

		brigade.Value.Should().Be(identifier);
	}

	[Fact]
	public void Renders_a_brigade_or_agency_identifier_as_invariant_text()
	{
		var identifier = BrigadeOrAgencyIdentifier.FromValue(26);

		identifier.ToString().Should().Be("26");
	}

	[Theory]
	[InlineData((byte)0)]
	[InlineData(byte.MaxValue)]
	public void Serializes_the_full_brigade_or_agency_octet_range(byte value)
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(value)),
			Node.FromValue(NodeIdentifier.FromValue(0)),
			Port.FromValue(PortIdentifier.FromValue(0)));

		address.ToWireValue()[0].Should().Be(value);
	}
}
