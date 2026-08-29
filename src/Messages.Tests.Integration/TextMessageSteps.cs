using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class TextMessageSteps
{
	private Block? block;
	private OfBlocks? ofBlocks;
	private Stensones.GD92.Fields.Text? text;
	private Stensones.GD92.Messages.Text? message;
	private byte[]? encodedContents;

	[Given(@"Block (.*) of (.*) containing the text ""(.*)""")]
	public void GivenBlockOfContainingTheText(byte block, byte ofBlocks, string text)
	{
		this.block = Block.FromValue(block);
		this.ofBlocks = OfBlocks.FromValue(ofBlocks);
		this.text = Stensones.GD92.Fields.Text.FromValue(text);
	}

	[When(@"a Text message is created")]
	public void WhenATextMessageIsCreated()
	{
		this.message = Stensones.GD92.Messages.Text.FromFields(this.block!, this.ofBlocks!, this.text!);
	}

	[Then(@"its message bytes are ""(.*)""")]
	public void ThenItsMessageBytesAre(string expectedBytes)
	{
		this.message!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Then(@"its Message Type bytes are ""(.*)""")]
	public void ThenItsMessageTypeBytesAre(string expectedBytes)
	{
		this.message!.Type.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Given(@"Text message contents bytes ""(.*)""")]
	public void GivenTextMessageContentsBytes(string encodedContents)
	{
		this.encodedContents = Convert.FromHexString(encodedContents);
	}

	[When(@"the Text message contents are decoded")]
	public void WhenTheTextMessageContentsAreDecoded()
	{
		var buffer = new EncodedMessageBuffer(this.encodedContents!);

		this.message = Stensones.GD92.Messages.Text.FromEncodedMessageBuffer(ref buffer);
	}

	[Then(@"its decoded block is (.*)")]
	public void ThenItsDecodedBlockIs(byte expectedBlock)
	{
		this.message!.Block.Value.Should().Be(expectedBlock);
	}

	[Then(@"its decoded number of blocks is (.*)")]
	public void ThenItsDecodedNumberOfBlocksIs(byte expectedOfBlocks)
	{
		this.message!.OfBlocks.Value.Should().Be(expectedOfBlocks);
	}

	[Then(@"its decoded text is ""(.*)""")]
	public void ThenItsDecodedTextIs(string expectedText)
	{
		this.message!.MessageText.Value.Should().Be(expectedText);
	}
}
