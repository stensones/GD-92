using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Tests.Unit.Router.Participants;

public sealed class InventoryScanTests
{
	[Fact]
	public void Does_not_find_an_unknown_Inventory_Scan()
	{
		var inventoryScan = new InventoryScan(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			new CompletingTransactionService(),
			new NonStoppingApplicationLifetime());

		var status = inventoryScan.Get(InventoryScanStatusIdentifier.Create());

		status.Should().BeNull();
	}

	[Fact]
	public async Task Starts_with_the_local_Router_and_no_completed_probes()
	{
		var transactions = new BlockingTransactionService();
		var inventoryScan = new InventoryScan(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			transactions,
			new NonStoppingApplicationLifetime());

		var identifier = inventoryScan.Start();
		var status = inventoryScan.Get(identifier);

		status.Should().NotBeNull();
		status!.CompletedProbeCount.Should().Be(0);
		status.Participants.Should().ContainSingle().Which.Should().Be(
			new InventoryParticipant(0, "router", null));
		status.Summary.Should().Be(InventoryScanSummary.Empty);

		transactions.Complete();
		await WaitForCompletionAsync(inventoryScan, identifier);
	}

	[Fact]
	public async Task Summarizes_completed_probe_outcomes()
	{
		var inventoryScan = new InventoryScan(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			new CompletingTransactionService(),
			new NonStoppingApplicationLifetime());

		var identifier = inventoryScan.Start();
		var status = await WaitForCompletionAsync(inventoryScan, identifier);

		status.Participants.Should().Contain(new InventoryParticipant(
			Port: 1,
			Kind: "mta",
			AgentType: "LAN MTA (10)"));
		status.Summary.DiscoveredParticipantCount.Should().Be(1);
		status.Summary.TimeoutCount.Should().Be(60);
		status.Summary.DeliveryFailureCount.Should().Be(1);
		status.Summary.NegativeAcknowledgements.Should().ContainSingle()
			.Which.Should().Be(new KeyValuePair<string, int>("parameter:invalid_syntax", 1));
	}

	[Fact]
	public async Task Starts_all_probes_concurrently_when_configured_for_all_participant_ports()
	{
		var transactions = new BlockingTransactionService();
		var inventoryScan = new InventoryScan(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(
				new ConfigurationBuilder()
					.AddInMemoryCollection(new Dictionary<string, string?>
					{
						["InventoryScan:MaximumConcurrentProbes"] = "63"
					})
					.Build()),
			transactions,
			new NonStoppingApplicationLifetime());

		var identifier = inventoryScan.Start();
		await transactions.WaitForAllProbesAsync();

		transactions.Complete();
		var status = await WaitForCompletionAsync(inventoryScan, identifier);

		status.CompletedProbeCount.Should().Be(63);
	}

	[Fact]
	public async Task Keeps_overlapping_Inventory_Scans_independently_addressable()
	{
		var transactions = new BlockingTransactionService();
		var inventoryScan = new InventoryScan(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			transactions,
			new NonStoppingApplicationLifetime());

		var firstIdentifier = inventoryScan.Start();
		var secondIdentifier = inventoryScan.Start();

		firstIdentifier.ToString().Should().NotBe(secondIdentifier.ToString());
		inventoryScan.Get(firstIdentifier).Should().NotBeNull();
		inventoryScan.Get(secondIdentifier).Should().NotBeNull();

		transactions.Complete();
		var firstStatus = await WaitForCompletionAsync(inventoryScan, firstIdentifier);
		var secondStatus = await WaitForCompletionAsync(inventoryScan, secondIdentifier);

		firstStatus.CompletedProbeCount.Should().Be(63);
		secondStatus.CompletedProbeCount.Should().Be(63);
	}

	private static async Task<InventoryScanStatus> WaitForCompletionAsync(
		InventoryScan inventoryScan,
		InventoryScanStatusIdentifier identifier)
	{
		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = inventoryScan.Get(identifier);
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

	private sealed class CompletingTransactionService : IManagementTransactionService
	{
		public Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
			ManagementTransactionRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult(new RouterParameterRequestStatusIdentifier(
				new UniqueSystemWideReference(
					request.Source,
					request.Destination,
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)))));
		}

		public Task<RouterParameterRequestStatus> WaitForCompletionAsync(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			CancellationToken cancellationToken)
		{
			var port = statusIdentifier.USWR.Destination.Port.Value;
			return Task.FromResult<RouterParameterRequestStatus>(port switch
			{
				1 => new ReceivedRouterParameterRequestStatus(
					statusIdentifier,
					MoreValues.No,
					ParameterValue.FromWireValue([10])),
				2 => new RejectedRouterParameterRequestStatus(
					statusIdentifier,
					ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax)),
				3 => new DeliveryFailedRouterParameterRequestStatus(statusIdentifier),
				_ => new TimedOutRouterParameterRequestStatus(statusIdentifier)
			});
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
