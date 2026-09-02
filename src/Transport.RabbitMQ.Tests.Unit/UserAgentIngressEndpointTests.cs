using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class UserAgentIngressEndpointTests
{
	[Fact]
	public void Derives_a_listener_queue_name_from_the_full_User_Agent_address()
	{
		var userAgentAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(25)));

		var queueName = UserAgentIngressEndpoint.QueueNameFrom(userAgentAddress);

		queueName.Should().Be("gd92.ua.26.100.25");
	}
}
