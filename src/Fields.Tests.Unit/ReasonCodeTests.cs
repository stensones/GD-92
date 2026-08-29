using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ReasonCodeTests
{
	[Fact]
	public void Serializes_the_general_invalid_message_reason_code()
	{
		var reasonCode = ReasonCode.FromGeneralReasonCode(GeneralReasonCode.InvalidMessage);

		reasonCode.ToWireValue().Should().Equal(new byte[] { 0x01, 0x03 });
	}
}
