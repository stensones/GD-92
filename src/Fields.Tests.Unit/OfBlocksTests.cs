using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class OfBlocksTests
{
	[Fact]
	public void Serializes_its_value_as_one_byte()
	{
		var ofBlocks = OfBlocks.FromValue(1);

		ofBlocks.ToWireValue().Should().Equal(new byte[] { 0x01 });
	}

	[Fact]
	public void Is_a_word8_field()
	{
		var ofBlocks = OfBlocks.FromValue(1);

		ofBlocks.Should().BeAssignableTo<Word8>();
	}
}
