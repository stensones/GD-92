using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ResetReasonTests
{
	[Theory]
	[InlineData(ResetReasonValue.RequestedByResetRequest, 0x00)]
	[InlineData(ResetReasonValue.SoftwareFailure, 0x01)]
	[InlineData(ResetReasonValue.PowerOn, 0x02)]
	public void Serializes_each_defined_reset_reason(ResetReasonValue value, byte expectedWireValue)
	{
		ResetReason.FromValue(value).ToWireValue().Should().Equal(new byte[] { expectedWireValue });
	}
}
