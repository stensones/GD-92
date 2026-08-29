using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class BlockCheckCharacterTests
{
	[Fact]
	public void Calculates_the_exclusive_OR_of_all_preceding_envelope_bytes()
	{
		var envelopeBytes = Convert.FromHexString("1A191902011A191912FCD11B0101000446495245");

		BlockCheckCharacter.FromEnvelopeBytes(envelopeBytes).ToWireValue().Should().Equal(new byte[] { 0x3B });
	}

	[Fact]
	public void Creates_a_block_check_character_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x3B });

		BlockCheckCharacter.FromEncodedMessageBuffer(ref buffer).ToWireValue().Should().Equal(new byte[] { 0x3B });
		buffer.BitPosition.Should().Be(8);
	}
}
