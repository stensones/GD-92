using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class BlockCheckCharacterSteps
{
	private byte[]? envelopeBytes;
	private BlockCheckCharacter? blockCheckCharacter;

	[Given(@"the Envelope bytes before BCC are ""(.*)""")]
	public void GivenTheEnvelopeBytesBeforeBCCAre(string envelopeBytes)
	{
		this.envelopeBytes = Convert.FromHexString(envelopeBytes);
	}

	[When(@"the Block Check Character is calculated")]
	public void WhenTheBlockCheckCharacterIsCalculated()
	{
		this.blockCheckCharacter = BlockCheckCharacter.FromEnvelopeBytes(this.envelopeBytes!);
	}

	[Then(@"its Block Check Character field bytes are ""(.*)""")]
	public void ThenItsBlockCheckCharacterFieldBytesAre(string expectedBytes)
	{
		this.blockCheckCharacter!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
