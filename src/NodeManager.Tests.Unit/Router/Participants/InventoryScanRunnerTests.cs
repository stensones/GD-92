using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Tests.Unit.Router.Participants;

public sealed class InventoryScanRunnerTests
{
	[Fact]
	public async Task Records_a_rejected_probe_as_a_Negative_Acknowledgement()
	{
		var registry = new InMemoryInventoryScanRegistry();
		var services = new ServiceCollection();
		services.AddScoped<IManagementTransactionService, RejectingTransactionService>();
		await using var serviceProvider = services.BuildServiceProvider();
		var runner = new InventoryScanRunner(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			registry,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new NonStoppingApplicationLifetime());

		var identifier = runner.Start();
		var status = await WaitForCompletedScanAsync(registry, identifier);

		status.Summary.NegativeAcknowledgements.Should().ContainSingle()
			.Which.Should().Be(new KeyValuePair<string, int>("parameter:invalid_syntax", 1));
		status.Summary.TimeoutCount.Should().Be(62);
	}

	[Fact]
	public async Task Starts_all_probes_concurrently_when_configured_for_all_participant_ports()
	{
		var registry = new InMemoryInventoryScanRegistry();
		var transactions = new BlockingTransactionService();
		var services = new ServiceCollection();
		services.AddSingleton(transactions);
		services.AddScoped<IManagementTransactionService>(serviceProvider =>
			serviceProvider.GetRequiredService<BlockingTransactionService>());
		await using var serviceProvider = services.BuildServiceProvider();
		var runner = new InventoryScanRunner(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(
				new ConfigurationBuilder()
					.AddInMemoryCollection(new Dictionary<string, string?>
					{
						["InventoryScan:MaximumConcurrentProbes"] = "63"
					})
					.Build()),
			registry,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new NonStoppingApplicationLifetime());

		var identifier = runner.Start();
		await transactions.WaitForAllProbesAsync();

		transactions.Complete();
		var status = await WaitForCompletedScanAsync(registry, identifier);

		status.CompletedProbeCount.Should().Be(63);
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

	private static CommunicationsAddress Address(byte port) =>
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));

	private sealed class RejectingTransactionService : IManagementTransactionService
	{
		public Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
			ManagementTransactionRequest request,
			CancellationToken cancellationToken) =>
			Task.FromResult(new RouterParameterRequestStatusIdentifier(
				new UniqueSystemWideReference(
					request.Source,
					request.Destination,
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)))));

		public Task<RouterParameterRequestStatus> WaitForCompletionAsync(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			CancellationToken cancellationToken)
		{
			return Task.FromResult<RouterParameterRequestStatus>(
				statusIdentifier.USWR.Destination.Port.Value == 3
					? new RejectedRouterParameterRequestStatus(
						statusIdentifier,
						ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax))
					: new TimedOutRouterParameterRequestStatus(statusIdentifier));
		}
	}

	private sealed class BlockingTransactionService : IManagementTransactionService
	{
		private const int TotalParticipantPorts = 63;
		private readonly TaskCompletionSource allProbesSubmitted = new(
			TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly TaskCompletionSource completion = new(
			TaskCreationOptions.RunContinuationsAsynchronously);
		private int submittedProbeCount;

		public Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
			ManagementTransactionRequest request,
			CancellationToken cancellationToken)
		{
			if (Interlocked.Increment(ref this.submittedProbeCount) == TotalParticipantPorts)
			{
				this.allProbesSubmitted.TrySetResult();
			}

			return Task.FromResult(new RouterParameterRequestStatusIdentifier(
				new UniqueSystemWideReference(
					request.Source,
					request.Destination,
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)))));
		}

		public async Task<RouterParameterRequestStatus> WaitForCompletionAsync(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			CancellationToken cancellationToken)
		{
			await this.completion.Task.WaitAsync(cancellationToken);
			return new TimedOutRouterParameterRequestStatus(statusIdentifier);
		}

		public async Task WaitForAllProbesAsync()
		{
			await this.allProbesSubmitted.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}

		public void Complete()
		{
			this.completion.TrySetResult();
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
