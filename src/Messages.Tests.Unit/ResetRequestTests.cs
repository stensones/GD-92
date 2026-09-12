using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ResetRequestTests
{
	[Fact]
	public void Serializes_the_reset_type()
	{
		var resetRequest = ResetRequest.FromFields(
			ResetType.FromValue(ResetTypeValue.SoftwareReset));

		resetRequest.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Decodes_the_reset_type()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x00 });

		var resetRequest = ResetRequest.FromEncodedMessageBuffer(ref buffer);

		resetRequest.ResetType.Value.Should().Be(ResetTypeValue.SoftwareReset);
		buffer.RemainingBitCount.Should().Be(0);
	}
}
