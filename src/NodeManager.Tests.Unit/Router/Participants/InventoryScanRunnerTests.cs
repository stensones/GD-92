using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit.Router.Participants;

public sealed class InventoryScanRunnerTests
{
	[Fact]
	public async Task Records_a_rejected_probe_as_a_Negative_Acknowledgement()
	{
		var nodeManager = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var responses = new RouterParameterResponseReceiver(pendingDeliveries);
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ =>
			new NegativeAcknowledgingRouterIngress(responses));
		await using var serviceProvider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true });
		var inventoryScans = new InMemoryInventoryScanRegistry();
		var runner = new InventoryScanRunner(
			new RouterParameterRequestSettings(
				nodeManager,
				router,
				NodeLoginRetryPolicy.FromValues(
					NodeLoginNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
					NodeLoginTotalSends.FromValue(Word8.FromValue(1)))),
			inventoryScans,
			pendingDeliveries,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new ImmediatelyCompletingRetryDelay(),
			new NonStoppingApplicationLifetime());

		var identifier = runner.Start();
		var status = await WaitForCompletedScanAsync(inventoryScans, identifier);

		status.Summary.NegativeAcknowledgements.Should().ContainSingle()
			.Which.Should().Be(new KeyValuePair<string, int>("parameter:invalid_syntax", 1));
		status.Summary.TimeoutCount.Should().Be(62);
	}

	private static async Task<InventoryScanStatus> WaitForCompletedScanAsync(
		IInventoryScanRegistry inventoryScans,
		InventoryScanStatusIdentifier identifier)
	{
		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = inventoryScans.Get(identifier);
			if (status?.CompletedProbeCount == 63)
			{
				return status;
			}

			await Task.Delay(TimeSpan.FromMilliseconds(100));
		}

		throw new Xunit.Sdk.XunitException("The Inventory Scan did not complete.");
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class NegativeAcknowledgingRouterIngress(
		RouterParameterResponseReceiver responses) : IRouterIngress
	{
		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			if (envelope.Destinations.Addresses.Single().Port.Value != 3)
			{
				return Task.CompletedTask;
			}

			return responses.ReceiveAsync(
				Envelope.CreateNegativeAcknowledgement(
					envelope,
					envelope.Destinations.Addresses.Single(),
					envelope.ProtocolAndPriority.ProtocolVersion,
					Destinations.FromAddresses(envelope.Destinations.Addresses.Single()),
					ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax)),
				cancellationToken);
		}
	}

	private sealed class ImmediatelyCompletingRetryDelay : INodeLoginRetryDelay
	{
		public Task WaitAsync(
			NodeLoginNoAcknowledgementTimeout timeout,
			CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}

	private sealed class NonStoppingApplicationLifetime : IHostApplicationLifetime
	{
		public CancellationToken ApplicationStarted => CancellationToken.None;
		public CancellationToken ApplicationStopping => CancellationToken.None;
		public CancellationToken ApplicationStopped => CancellationToken.None;

		public void StopApplication()
		{
		}
	}
}
