using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class AcknowledgementSteps
{
	private Acknowledgement? acknowledgement;

	[When(@"an Acknowledgement is created")]
	public void WhenAnAcknowledgementIsCreated()
	{
		this.acknowledgement = Acknowledgement.Create();
	}

	[Then(@"its Message Type bytes are ""(.*)""")]
	public void ThenItsMessageTypeBytesAre(string expectedBytes)
	{
		this.acknowledgement!.Type.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Then(@"its Contents bytes are empty")]
	public void ThenItsContentsBytesAreEmpty()
	{
		this.acknowledgement!.ToWireValue().Should().BeEmpty();
	}
}
