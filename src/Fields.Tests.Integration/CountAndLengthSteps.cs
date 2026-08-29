using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class CountAndLengthSteps
{
	private MessageLength? messageLength;
	private DestinationCount? destinationCount;
	private CountAndLength? countAndLength;

	[Given(@"a message length of (.*) bytes and (.*) destination")]
	public void GivenAMessageLengthOfBytesAndDestination(ushort messageLength, byte destinationCount)
	{
		this.messageLength = MessageLength.FromValue(messageLength);
		this.destinationCount = DestinationCount.FromValue(destinationCount);
	}

	[When(@"a CountAndLength field is created")]
	public void WhenACountAndLengthFieldIsCreated()
	{
		this.countAndLength = CountAndLength.FromValues(this.messageLength!, this.destinationCount!);
	}

	[Then(@"its count and length field bytes are ""(.*)""")]
	public void ThenItsCountAndLengthFieldBytesAre(string expectedBytes)
	{
		this.countAndLength!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
