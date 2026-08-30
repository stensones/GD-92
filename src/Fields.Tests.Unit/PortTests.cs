using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PortTests
{
	[Fact]
	public void Accepts_the_highest_valid_port_value()
	{
		Port.FromValue(PortIdentifier.FromValue(63)).Value.Should().Be((byte)63);
	}

	[Fact]
	public void Rejects_port_identifiers_above_the_protocol_range()
	{
		Action createPortIdentifier = () => PortIdentifier.FromValue(64);

		createPortIdentifier.Should().Throw<ArgumentOutOfRangeException>();
	}
}
