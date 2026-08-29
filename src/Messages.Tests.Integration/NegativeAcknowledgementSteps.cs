using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class NegativeAcknowledgementSteps
{
	private Destinations? destinations;
	private ReasonCode? reasonCode;
	private NegativeAcknowledgement? negativeAcknowledgement;

	[Given(@"a Negative Acknowledgement destination Brigade (.*), Node (.*), Port (.*)")]
	public void GivenANegativeAcknowledgementDestination(byte brigade, ushort node, byte port)
	{
		var destination = CommunicationsAddress.FromValues(
			Brigade.FromValue(brigade),
			Node.FromValue(node),
			Port.FromValue(port));

		this.destinations = Destinations.FromAddresses(destination);
	}

	[Given(@"the General Reason Code ""(.*)""")]
	public void GivenTheGeneralReasonCode(string reasonCode)
	{
		this.reasonCode = ReasonCode.FromGeneralReasonCode(
			Enum.Parse<GeneralReasonCode>(reasonCode, ignoreCase: true));
	}

	[When(@"a Negative Acknowledgement is created")]
	public void WhenANegativeAcknowledgementIsCreated()
	{
		this.negativeAcknowledgement = NegativeAcknowledgement.FromValues(
			this.destinations!,
			this.reasonCode!);
	}

	[Then(@"its Negative Acknowledgement Message Type bytes are ""(.*)""")]
	public void ThenItsNegativeAcknowledgementMessageTypeBytesAre(string expectedBytes)
	{
		this.negativeAcknowledgement!.Type.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Then(@"its Negative Acknowledgement Contents bytes are ""(.*)""")]
	public void ThenItsNegativeAcknowledgementContentsBytesAre(string expectedBytes)
	{
		this.negativeAcknowledgement!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
