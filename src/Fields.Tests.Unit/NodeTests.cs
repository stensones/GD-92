using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class NodeTests
{
	[Fact]
	public void Accepts_the_highest_valid_node_value()
	{
		Node.FromValue(NodeIdentifier.FromValue(1023)).Value.Should().Be((ushort)1023);
	}

	[Fact]
	public void Rejects_node_identifiers_above_the_protocol_range()
	{
		Action createNodeIdentifier = () => NodeIdentifier.FromValue(1024);

		createNodeIdentifier.Should().Throw<ArgumentOutOfRangeException>();
	}
}
