using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class NegativeAcknowledgementTests
{
	[Fact]
	public void Serializes_one_destination_and_the_general_invalid_message_reason()
	{
		var destination = CommunicationsAddress.FromValues(Brigade.FromValue(26), Node.FromValue(100), Port.FromValue(25));
		var negativeAcknowledgement = NegativeAcknowledgement.FromValues(
			Destinations.FromAddresses(destination),
			ReasonCode.FromGeneralReasonCode(GeneralReasonCode.InvalidMessage));

		negativeAcknowledgement.Type.ToWireValue().Should().Equal(new byte[] { 0x33 });
		negativeAcknowledgement.ToWireValue().Should().Equal(Convert.FromHexString("011A19190103"));
	}
}
