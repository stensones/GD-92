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
}
