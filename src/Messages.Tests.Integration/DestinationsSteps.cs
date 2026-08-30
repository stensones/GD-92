using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class DestinationsSteps
{
	private CommunicationsAddress? firstAddress;
	private CommunicationsAddress? secondAddress;
	private Destinations? destinations;

	[Given(@"destination addresses Brigade (.*), Node (.*), Port (.*) and Brigade (.*), Node (.*), Port (.*)")]
	public void GivenDestinationAddresses(
		byte firstBrigade,
		ushort firstNode,
		byte firstPort,
		byte secondBrigade,
		ushort secondNode,
		byte secondPort)
	{
		this.firstAddress = CreateAddress(firstBrigade, firstNode, firstPort);
		this.secondAddress = CreateAddress(secondBrigade, secondNode, secondPort);
	}

	[When(@"Envelope destinations are created")]
	public void WhenEnvelopeDestinationsAreCreated()
	{
		this.destinations = Destinations.FromAddresses(this.firstAddress!, this.secondAddress!);
	}

	[Then(@"their destination bytes are ""(.*)""")]
	public void ThenTheirDestinationBytesAre(string expectedBytes)
	{
		this.destinations!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Then(@"their Destination Count is (.*)")]
	public void ThenTheirDestinationCountIs(byte expectedCount)
	{
		this.destinations!.Count.Value.Should().Be(expectedCount);
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
