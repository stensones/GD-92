using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class DestinationCountTests
{
	[Fact]
	public void Creates_a_destination_count()
	{
		DestinationCount.FromValue(DestinationAddressCount.FromValue(1)).Value.Should().Be((byte)1);
	}

	[Theory]
	[InlineData((byte)0)]
	[InlineData((byte)64)]
	public void Rejects_destination_address_counts_outside_the_protocol_range(byte value)
	{
		Action createDestinationAddressCount = () => DestinationAddressCount.FromValue(value);

		createDestinationAddressCount.Should().Throw<ArgumentOutOfRangeException>();
	}
}
