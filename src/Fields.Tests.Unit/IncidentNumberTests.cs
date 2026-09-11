using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class IncidentNumberTests
{
	[Fact]
	public void Serializes_an_incident_number_as_a_big_endian_word32()
	{
		IncidentNumber.FromValue(0x01020304).ToWireValue().Should()
			.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 });
	}

	[Fact]
	public void Creates_an_incident_number_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x01, 0x02, 0x03, 0x04 });

		IncidentNumber.FromEncodedMessageBuffer(ref buffer).Value.Should().Be(0x01020304);
		buffer.BitPosition.Should().Be(32);
	}
}
