using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class TextTests
{
	[Fact]
	public void Is_GD92_message_contents()
	{
		typeof(Text).GetInterfaces().Should().Contain(typeof(IGD92MessageContents));
	}

	[Fact]
	public void Has_message_type_27()
	{
		IGD92MessageContents messageContents = Text.FromFields(
			Block.FromValue(1),
			OfBlocks.FromValue(1),
			Stensones.GD92.Fields.Text.FromValue("FIRE"));

		messageContents.Type.ToWireValue().Should().Equal(new byte[] { 0x1B });
	}

	[Fact]
	public void Creates_text_contents_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("0101000446495245"));

		var contents = Text.FromEncodedMessageBuffer(ref buffer);

		contents.ToWireValue().Should().Equal(Convert.FromHexString("0101000446495245"));
		buffer.BitPosition.Should().Be(64);
	}

	[Fact]
	public void Serializes_its_block_of_blocks_and_text_in_protocol_order()
	{
		var message = Text.FromFields(
			Block.FromValue(1),
			OfBlocks.FromValue(1),
			Stensones.GD92.Fields.Text.FromValue("FIRE"));

		message.ToWireValue().Should().Equal(Convert.FromHexString("0101000446495245"));
	}
}
