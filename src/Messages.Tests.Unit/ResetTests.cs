using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ResetTests
{
	[Fact]
	public void Serializes_the_reset_reason()
	{
		var reset = Reset.FromFields(
			ResetReason.FromValue(ResetReasonValue.PowerOn));

		reset.ToWireValue().Should().Equal(new byte[] { 0x02 });
	}

	[Fact]
	public void Decodes_the_reset_reason()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x02 });

		var reset = Reset.FromEncodedMessageBuffer(ref buffer);

		reset.ResetReason.Value.Should().Be(ResetReasonValue.PowerOn);
		buffer.RemainingBitCount.Should().Be(0);
	}
}
