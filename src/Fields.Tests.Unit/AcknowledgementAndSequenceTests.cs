using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class AcknowledgementAndSequenceTests
{
	[Fact]
	public void Serializes_a_requested_acknowledgement_and_sequence()
	{
		var field = AcknowledgementAndSequence.FromValues(
			SequenceNumber.FromValue(31953),
			AcknowledgementRequest.Requested);

		field.ToWireValue().Should().Equal(new byte[] { 0xFC, 0xD1 });
	}

	[Fact]
	public void Creates_an_acknowledgement_and_sequence_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0xFC, 0xD1 });

		AcknowledgementAndSequence.FromEncodedMessageBuffer(ref buffer).ToWireValue().Should().Equal(new byte[] { 0xFC, 0xD1 });
		buffer.BitPosition.Should().Be(16);
	}
}
