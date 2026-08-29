using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PortTests
{
	[Fact]
	public void Accepts_the_highest_valid_port_value()
	{
		Port.FromValue(63).Value.Should().Be((byte)63);
	}

	[Fact]
	public void Rejects_values_above_the_port_range()
	{
		Action createPort = () => Port.FromValue(64);

		createPort.Should().Throw<ArgumentOutOfRangeException>();
	}
}
