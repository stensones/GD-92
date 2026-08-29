using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MessageTypeTests
{
	[Fact]
	public void Creates_the_text_message_type_from_its_semantic_value()
	{
		MessageType.FromValue(GD92MessageType.Text).ToWireValue().Should().Equal(new byte[] { 0x1B });
	}

	[Fact]
	public void Rejects_a_message_type_that_is_not_defined_by_GD92()
	{
		var undefinedMessageType = (GD92MessageType)11;
		var createMessageType = () => MessageType.FromValue(undefinedMessageType);

		createMessageType.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Rejects_an_encoded_message_type_that_is_not_defined_by_GD92()
	{
		var decodeMessageType = () => DecodeMessageType(0x0B);

		decodeMessageType.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Creates_a_message_type_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x1B });

		MessageType.FromEncodedMessageBuffer(ref buffer).ToWireValue().Should().Equal(new byte[] { 0x1B });
		buffer.BitPosition.Should().Be(8);
	}

	private static void DecodeMessageType(byte encodedValue)
	{
		var buffer = new EncodedMessageBuffer(new byte[] { encodedValue });

		MessageType.FromEncodedMessageBuffer(ref buffer);
	}
}
