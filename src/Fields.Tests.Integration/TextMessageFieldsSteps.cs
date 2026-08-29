using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class TextMessageFieldsSteps
{
	private byte blockNumber;
	private byte numberOfBlocks;
	private string? text;
	private byte[]? serializedBodyPayload;

	[Given(@"text message block (.*) of (.*) containing ""(.*)""")]
	public void GivenTextMessageBlockOfContaining(byte blockNumber, byte numberOfBlocks, string text)
	{
		this.blockNumber = blockNumber;
		this.numberOfBlocks = numberOfBlocks;
		this.text = text;
	}

	[When(@"the text message fields are created and serialized in protocol order")]
	public void WhenTheTextMessageFieldsAreCreatedAndSerializedInProtocolOrder()
	{
		var block = Block.FromValue(this.blockNumber);
		var ofBlocks = OfBlocks.FromValue(this.numberOfBlocks);
		var messageText = Text.FromValue(this.text!);

		this.serializedBodyPayload = [.. block.ToWireValue(), .. ofBlocks.ToWireValue(), .. messageText.ToWireValue()];
	}

	[Then(@"the text message body payload is ""(.*)""")]
	public void ThenTheTextMessageBodyPayloadIs(string expectedPayload)
	{
		this.serializedBodyPayload.Should().Equal(Convert.FromHexString(expectedPayload));
	}
}
