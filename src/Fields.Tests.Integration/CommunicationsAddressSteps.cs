using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class CommunicationsAddressSteps
{
	private Brigade? brigade;
	private Node? node;
	private Port? port;
	private CommunicationsAddress? address;

	[Given(@"valid Brigade (.*), Node (.*), and Port (.*) values")]
	public void GivenBrigadeNodeAndPort(byte brigade, ushort node, byte port)
	{
		this.brigade = Brigade.FromValue(brigade);
		this.node = Node.FromValue(NodeIdentifier.FromValue(node));
		this.port = Port.FromValue(PortIdentifier.FromValue(port));
	}

	[When(@"a Communications Address is created")]
	public void WhenACommunicationsAddressIsCreated()
	{
		this.address = CommunicationsAddress.FromValues(this.brigade!, this.node!, this.port!);
	}

	[Then(@"its serialized field bytes are ""(.*)""")]
	public void ThenItsSerializedFieldBytesAre(string expectedBytes)
	{
		this.address!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
