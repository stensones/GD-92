using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Router.Persistence;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Router.Tests.Unit;

public sealed class RouterIngressReceiverTests
{
	[Fact]
	public async Task Delegates_a_received_Envelope_to_Router_Local_Delivery()
	{
		var routerAddress = RouterParameterModuleTestSupport.CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(
			RouterParameterModuleTestSupport.CreateCurrentParameters(routerAddress, 42));
		var userAgentIngress = new CapturingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterLocalDelivery(
				routerAddress,
				new RouterParameterRead(
					routerAddress,
					RouterParameterModuleTestSupport.ProtocolVersion,
					currentParameters),
				new NodeLogin(
					routerAddress,
					RouterParameterModuleTestSupport.ProtocolVersion,
					currentParameters),
				new Level1PasswordModification(
					routerAddress,
					RouterParameterModuleTestSupport.ProtocolVersion,
					currentParameters,
					new InMemoryPasswordVerifierStore()),
				userAgentIngress,
				new NoOpLocalParticipantIngress(),
				NullLogger<RouterLocalDelivery>.Instance),
			NullLogger<RouterIngressReceiver>.Instance);

		await receiver.ReceiveAsync(
			RouterParameterModuleTestSupport.CreateParameterRequest(
				routerAddress,
				Stensones.GD92.Fields.ParameterTable.Current),
			CancellationToken.None);

		userAgentIngress.Envelope!.Contents.Should().BeOfType<Parameter>();
	}

	private sealed class CapturingUserAgentIngress : IUserAgentIngress
	{
		public Envelope? Envelope { get; private set; }

		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class NoOpLocalParticipantIngress : ILocalParticipantIngress
	{
		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}
}
