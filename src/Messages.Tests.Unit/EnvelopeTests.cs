using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class EnvelopeTests
{
	[Fact]
	public void Creates_an_envelope_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(
			Convert.FromHexString("1A191902011A191912FCD11B01010004464952453B"));

		var envelope = Envelope.FromEncodedMessageBuffer(ref buffer);

		envelope.ToWireValue().Should().Equal(
			Convert.FromHexString("1A191902011A191912FCD11B01010004464952453B"));
		buffer.BitPosition.Should().Be(168);
	}

	[Fact]
	public void Rejects_an_envelope_with_a_mismatched_block_check_character()
	{
		var decodeEnvelope = () => DecodeEnvelope("1A191902011A191912FCD11B01010004464952453A");

		decodeEnvelope.Should().Throw<InvalidOperationException>();
	}

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

	private static void DecodeEnvelope(string encodedEnvelope)
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString(encodedEnvelope));

		Envelope.FromEncodedMessageBuffer(ref buffer);
	}
}
