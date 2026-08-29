using System.Text;
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

	[Fact]
	public void Compresses_runs_of_more_than_three_identical_characters()
	{
		var text = Text.FromValue("     HIGH STREET");

		text.ToWireValue().Should().Equal(new byte[] { 0x00, 0x0E, 0x1B, 0x20, 0x05, 0x48, 0x49, 0x47, 0x48, 0x20, 0x53, 0x54, 0x52, 0x45, 0x45, 0x54 });
	}

	[Fact]
	public void Escapes_a_literal_escape_character()
	{
		var text = Text.FromValue("\x1B");

		text.ToWireValue().Should().Equal(new byte[] { 0x00, 0x03, 0x1B, 0x1B, 0x01 });
	}

	[Fact]
	public void Serializes_the_highest_seven_bit_ascii_character()
	{
		var text = Text.FromValue("\x7F");

		text.ToWireValue().Should().Equal(new byte[] { 0x00, 0x01, 0x7F });
	}

	[Fact]
	public void Rejects_characters_outside_seven_bit_ascii()
	{
		Action createText = () => Text.FromValue("\u0080");

		createText.Should().Throw<EncoderFallbackException>();
	}

	[Fact]
	public void Uses_the_compressed_length_for_long_text()
	{
		var text = Text.FromValue(new string('A', ushort.MaxValue + 1));

		var wireValue = text.ToWireValue();

		wireValue.Should().HaveCount(774);
		wireValue[..2].Should().Equal(new byte[] { 0x03, 0x04 });
	}
}
