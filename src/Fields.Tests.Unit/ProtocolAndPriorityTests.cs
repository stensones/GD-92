using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ProtocolAndPriorityTests
{
	[Fact]
	public void Serializes_priority_then_protocol_version()
	{
		var field = ProtocolAndPriority.FromValues(
			MessagePriority.FromValue(1),
			ProtocolVersion.FromValue(2));

		field.ToWireValue().Should().Equal(new byte[] { 0x12 });
	}
}
