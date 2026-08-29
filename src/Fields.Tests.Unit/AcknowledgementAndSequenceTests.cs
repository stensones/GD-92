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
}
