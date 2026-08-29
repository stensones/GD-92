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
}
