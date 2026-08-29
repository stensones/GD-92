using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class NodeTests
{
	[Fact]
	public void Accepts_the_highest_valid_node_value()
	{
		Node.FromValue(1023).Value.Should().Be((ushort)1023);
	}

	[Fact]
	public void Rejects_values_above_the_node_range()
	{
		Action createNode = () => Node.FromValue(1024);

		createNode.Should().Throw<ArgumentOutOfRangeException>();
	}
}
