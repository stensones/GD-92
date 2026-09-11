using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ManualAcknowledgementRequestTests
{
	[Fact]
	public void Serializes_a_required_manual_acknowledgement()
	{
		ManualAcknowledgementRequest.Required.ToWireValue().Should().Equal(new byte[] { 0x01 });
	}

	[Fact]
	public void Serializes_a_manual_acknowledgement_that_is_not_required()
	{
		ManualAcknowledgementRequest.NotRequired.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Creates_a_manual_acknowledgement_request_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x01 });

		ManualAcknowledgementRequest.FromEncodedMessageBuffer(ref buffer).Should()
			.Be(ManualAcknowledgementRequest.Required);
		buffer.BitPosition.Should().Be(8);
	}

	[Fact]
	public void Rejects_an_undefined_manual_acknowledgement_encoding()
	{
		var decodeRequest = () => DecodeManualAcknowledgementRequest(0x02);

		decodeRequest.Should().Throw<ArgumentOutOfRangeException>();
	}

	private static ManualAcknowledgementRequest DecodeManualAcknowledgementRequest(byte value)
	{
		var buffer = new EncodedMessageBuffer(new byte[] { value });

		return ManualAcknowledgementRequest.FromEncodedMessageBuffer(ref buffer);
	}
}
