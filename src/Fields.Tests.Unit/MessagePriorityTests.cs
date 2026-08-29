using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MessagePriorityTests
{
	[Theory]
	[InlineData((byte)1)]
	[InlineData((byte)9)]
	public void Accepts_valid_priorities(byte value)
	{
		MessagePriority.FromValue(value).Value.Should().Be(value);
	}

	[Theory]
	[InlineData((byte)0)]
	[InlineData((byte)10)]
	public void Rejects_invalid_priorities(byte value)
	{
		Action createPriority = () => MessagePriority.FromValue(value);

		createPriority.Should().Throw<ArgumentOutOfRangeException>();
	}
}
