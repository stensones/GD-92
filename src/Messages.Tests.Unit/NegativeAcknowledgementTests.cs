using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class NegativeAcknowledgementTests
{
	[Fact]
	public void Serializes_one_destination_and_the_general_invalid_message_reason()
	{
		var destination = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(25)));
		var negativeAcknowledgement = NegativeAcknowledgement.FromValues(
			Destinations.FromAddresses(destination),
			ReasonCode.FromGeneralReasonCode(GeneralReasonCode.InvalidMessage));

		negativeAcknowledgement.Type.ToWireValue().Should().Equal(new byte[] { 0x33 });
		negativeAcknowledgement.ToWireValue().Should().Equal(Convert.FromHexString("011A19190103"));
	}

	[Fact]
	public void Decodes_one_destination_and_the_general_invalid_message_reason()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("011A19190103"));

		var negativeAcknowledgement = NegativeAcknowledgement.FromEncodedMessageBuffer(ref buffer);

		negativeAcknowledgement.Destinations.Addresses.Should().ContainSingle()
			.Which.Should().Be(CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
				Node.FromValue(NodeIdentifier.FromValue(100)),
				Port.FromValue(PortIdentifier.FromValue(25))));
		negativeAcknowledgement.ReasonCode.GeneralReasonCode.Should().Be(GeneralReasonCode.InvalidMessage);
		buffer.BitPosition.Should().Be(48);
	}
}
