using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class TextTests
{
	[Fact]
	public void Serializes_uncompressed_ascii_with_a_big_endian_length()
	{
		var text = Text.FromValue("FIRE");

		text.ToWireValue().Should().Equal(new byte[] { 0x00, 0x04, 0x46, 0x49, 0x52, 0x45 });
	}
}
