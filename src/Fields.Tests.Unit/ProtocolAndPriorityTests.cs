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

	[Fact]
	public void Creates_priority_and_protocol_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x12 });

		ProtocolAndPriority.FromEncodedMessageBuffer(ref buffer).ToWireValue().Should().Equal(new byte[] { 0x12 });
		buffer.BitPosition.Should().Be(8);
	}
}
