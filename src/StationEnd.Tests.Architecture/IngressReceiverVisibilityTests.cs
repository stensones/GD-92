using AwesomeAssertions;
using NodeManager.Router.Parameters;
using Router;
using Stensones.GD92.Transport.RabbitMQ;

namespace Stensones.GD92.StationEnd.Tests.Architecture;

public sealed class IngressReceiverVisibilityTests
{
	[Fact]
	public void Router_ingress_receivers_are_public_for_Wolverine_code_generation()
	{
		typeof(IRouterIngressReceiver)
			.IsAssignableFrom(typeof(RouterIngressReceiver))
			.Should()
			.BeTrue();
		typeof(RouterIngressReceiver).IsPublic.Should().BeTrue();
	}

	[Fact]
	public void User_agent_ingress_receivers_are_public_for_Wolverine_code_generation()
	{
		typeof(IUserAgentIngressReceiver)
			.IsAssignableFrom(typeof(RouterParameterResponseReceiver))
			.Should()
			.BeTrue();
		typeof(RouterParameterResponseReceiver).IsPublic.Should().BeTrue();
	}
}
