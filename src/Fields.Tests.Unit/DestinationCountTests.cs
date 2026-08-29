using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class DestinationCountTests
{
	[Fact]
	public void Creates_a_destination_count()
	{
		DestinationCount.FromValue(1).Value.Should().Be((byte)1);
	}
}
