using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class TextTests
{
	[Fact]
	public void Is_a_GD92_message()
	{
		typeof(Text).GetInterfaces().Should().Contain(typeof(IGD92Message));
	}

	[Fact]
	public void Has_message_type_27()
	{
		IGD92Message message = Text.FromFields(
			Block.FromValue(1),
			OfBlocks.FromValue(1),
			Stensones.GD92.Fields.Text.FromValue("FIRE"));

		message.Type.ToWireValue().Should().Equal(new byte[] { 0x1B });
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
