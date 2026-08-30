using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MessageLengthTests
{
	[Fact]
	public void Creates_a_message_length_in_bytes()
	{
		MessageLength.FromValue(MessageByteLength.FromValue(8)).Value.Should().Be((ushort)8);
	}

	[Fact]
	public void Rejects_message_byte_lengths_above_the_protocol_limit()
	{
		Action createMessageByteLength = () => MessageByteLength.FromValue(1024);

		createMessageByteLength.Should().Throw<ArgumentOutOfRangeException>();
	}
}
