using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class RouterIngressEndpointTests
{
	[Fact]
	public void Derives_a_queue_URI_from_the_router_communications_node()
	{
		var routerAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(0)));

		var endpoint = RouterIngressEndpoint.From(routerAddress);

		endpoint.Should().Be(new Uri("rabbitmq://queue/gd92.router.26.100"));
	}
}
