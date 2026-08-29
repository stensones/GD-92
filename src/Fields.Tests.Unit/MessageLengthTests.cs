using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MessageLengthTests
{
	[Fact]
	public void Creates_a_message_length_in_bytes()
	{
		MessageLength.FromValue(8).Value.Should().Be((ushort)8);
	}
}
