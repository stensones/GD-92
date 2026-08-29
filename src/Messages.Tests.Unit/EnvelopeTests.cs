using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class EnvelopeTests
{
	[Fact]
	public void Serializes_a_complete_text_message_envelope()
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(26),
			Node.FromValue(100),
			Port.FromValue(25));
		var envelope = Envelope.FromValues(
			address,
			Destinations.FromAddresses(address),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(1),
				ProtocolVersion.FromValue(2)),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(31953),
				AcknowledgementRequest.Requested),
			Text.FromFields(
				Block.FromValue(1),
				OfBlocks.FromValue(1),
				Stensones.GD92.Fields.Text.FromValue("FIRE")));

		envelope.ToWireValue().Should().Equal(
			Convert.FromHexString("1A191902011A191912FCD11B01010004464952453B"));
	}
}
