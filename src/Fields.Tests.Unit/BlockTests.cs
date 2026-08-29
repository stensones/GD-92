using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class BlockTests
{
	[Fact]
	public void Serializes_its_value_as_one_byte()
	{
		var block = Block.FromValue(1);

		block.ToWireValue().Should().Equal(new byte[] { 0x01 });
	}

	[Fact]
	public void Is_a_word8_field()
	{
		var block = Block.FromValue(1);

		block.Should().BeAssignableTo<Word8>();
	}

	[Fact]
	public void Creates_a_block_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x01 });

		Block.FromEncodedMessageBuffer(ref buffer).ToWireValue().Should().Equal(new byte[] { 0x01 });
		buffer.BitPosition.Should().Be(8);
	}
}
