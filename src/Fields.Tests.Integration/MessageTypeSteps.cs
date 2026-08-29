using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class MessageTypeSteps
{
	private GD92MessageType type;
	private MessageType? messageType;

	[Given(@"the Text GD92 message type")]
	public void GivenTheTextGD92MessageType()
	{
		this.type = GD92MessageType.Text;
	}

	[When(@"a Message Type field is created")]
	public void WhenAMessageTypeFieldIsCreated()
	{
		this.messageType = MessageType.FromValue(this.type);
	}

	[Then(@"its field bytes are ""(.*)""")]
	public void ThenItsFieldBytesAre(string expectedBytes)
	{
		this.messageType!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
