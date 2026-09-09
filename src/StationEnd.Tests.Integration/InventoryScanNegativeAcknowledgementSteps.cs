using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class InventoryScanNegativeAcknowledgementSteps
{
	private ServiceProvider? serviceProvider;
	private IInventoryScanRegistry? inventoryScans;
	private IInventoryScanRunner? inventoryScanRunner;
	private InventoryScanStatusIdentifier? scanIdentifier;

	[Given(@"local participant port 3 rejects its Inventory Scan probe with invalid syntax")]
	public void GivenLocalParticipantPort3RejectsItsInventoryScanProbeWithInvalidSyntax()
	{
		var nodeManager = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var transactions = new InMemoryManagementTransactionRegistry();
		var responses = new RouterParameterResponseReceiver(transactions);
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ =>
			new NegativeAcknowledgingRouterIngress(responses));
		services.AddScoped<IManagementTransactionService>(serviceProvider =>
			new ManagementTransactionService(
				transactions,
				ManagementTransactionRetryPolicy.FromValues(
					ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
					ManagementTransactionTotalSends.FromValue(Word8.FromValue(1))),
				serviceProvider.GetRequiredService<IRouterIngress>(),
				serviceProvider.GetRequiredService<IServiceScopeFactory>(),
				new ImmediatelyCompletingRetryDelay(),
				new NonStoppingApplicationLifetime(),
				NullLogger<ManagementTransactionService>.Instance));
		this.serviceProvider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true });
		this.inventoryScans = new InMemoryInventoryScanRegistry();
		this.inventoryScanRunner = new InventoryScanRunner(
			new RouterParameterRequestSettings(
				nodeManager,
				router,
				ManagementTransactionRetryPolicy.FromValues(
					ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
					ManagementTransactionTotalSends.FromValue(Word8.FromValue(1)))),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			this.inventoryScans,
			this.serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new NonStoppingApplicationLifetime());
	}

	[When(@"NodeManager starts an Inventory Scan")]
	public void WhenNodeManagerStartsAnInventoryScan()
	{
		this.scanIdentifier = this.inventoryScanRunner!.Start();
	}

	[Then(@"the completed Inventory Scan summary records one invalid_syntax Negative Acknowledgement")]
	public async Task ThenTheCompletedInventoryScanSummaryRecordsOneInvalidSyntaxNegativeAcknowledgement()
	{
		var status = await this.WaitForCompletedScanAsync();

		status.Summary.NegativeAcknowledgements.Should().ContainSingle()
			.Which.Should().Be(new KeyValuePair<string, int>("parameter:invalid_syntax", 1));
	}

	[Then(@"the completed Inventory Scan summary records (.*) timeouts")]
	public async Task ThenTheCompletedInventoryScanSummaryRecordsTimeouts(int timeoutCount)
	{
		var status = await this.WaitForCompletedScanAsync();

		status.Summary.TimeoutCount.Should().Be(timeoutCount);
	}

	[AfterScenario]
	public void DisposeServiceProvider()
	{
		this.serviceProvider?.Dispose();
	}

	private async Task<InventoryScanStatus> WaitForCompletedScanAsync()
	{
		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = this.inventoryScans!.Get(this.scanIdentifier!);
			if (status?.CompletedProbeCount == 63)
			{
				return status;
			}

			await Task.Delay(TimeSpan.FromMilliseconds(100));
		}

		throw new Xunit.Sdk.XunitException(
			"The Inventory Scan did not complete.");
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

	private sealed class ImmediatelyCompletingRetryDelay : IManagementTransactionRetryDelay
	{
		public Task WaitAsync(
			ManagementTransactionNoAcknowledgementTimeout timeout,
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
