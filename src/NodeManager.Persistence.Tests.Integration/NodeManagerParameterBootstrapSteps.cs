using AwesomeAssertions;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodeManager.Persistence;
using ParticipantParameters;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Testcontainers.PostgreSql;

namespace NodeManager.Persistence.Tests.Integration;

[Binding]
public sealed class NodeManagerParameterBootstrapSteps
{
	private PostgreSqlContainer? database;
	private NodeManagerCurrentParameterProjection[]? concurrentProjections;
	private ParameterStoreWriteResult[]? concurrentWrites;
	private Barrier? parameterSetUpdateBarrier;

	[Given(@"an empty isolated NodeManager database")]
	public async Task GivenAnEmptyIsolatedNodeManagerDatabase()
	{
		this.database = new PostgreSqlBuilder("postgres:17.5").Build();
		await this.database.StartAsync();

		await using var context = this.CreateContext();
		await context.Database.MigrateAsync();
	}

	[When(@"NodeManager stores Non-Volatile Parameter 1 with value (.*)")]
	public async Task WhenNodeManagerStoresNonVolatileParameterWithValue(byte value)
	{
		await this.StoreAsync(ParameterTable.NonVolatile, value);
	}

	[When(@"NodeManager stores Permanent Parameter 1 with value (.*)")]
	public async Task WhenNodeManagerStoresPermanentParameterWithValue(byte value)
	{
		await this.StoreAsync(ParameterTable.Permanent, value);
	}

	[Then(@"its Permanent Parameter 1 has value (.*)")]
	public async Task ThenItsPermanentParameterHasValue(byte value)
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfNodeManagerParameterStore(context);

		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([value]);
	}

	[Then(@"its Permanent Parameter 1 is absent")]
	public async Task ThenItsPermanentParameterIsAbsent()
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfNodeManagerParameterStore(context);

		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(1)))
			.Should().BeNull();
	}

	[Then(@"its Non-Volatile Parameter 1 has value (.*)")]
	public async Task ThenItsNonVolatileParameterHasValue(byte value)
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfNodeManagerParameterStore(context);

		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([value]);
	}

	[Then(@"its Non-Volatile Parameter 1 is absent")]
	public async Task ThenItsNonVolatileParameterIsAbsent()
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfNodeManagerParameterStore(context);

		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(1)))
			.Should().BeNull();
	}

	[When(@"two NodeManager Parameter starts use different bootstrap configurations concurrently")]
	public async Task WhenTwoNodeManagerParameterStartsUseDifferentBootstrapConfigurationsConcurrently()
	{
		this.concurrentProjections = await Task.WhenAll(
			this.LoadProjectionAsync(CreateAddress(26, 100, 25)),
			this.LoadProjectionAsync(CreateAddress(26, 100, 24)));
	}

	[Then(@"both NodeManager starts project one complete first-committed Parameter set")]
	public void ThenBothNodeManagerStartsProjectOneCompleteFirstCommittedParameterSet()
	{
		this.concurrentProjections.Should().NotBeNull().And.HaveCount(2);

		var projectedPortNumbers = this.concurrentProjections!
			.Select(projection => projection.Get(NodeManagerParameterCatalogue.PortNumber).ToWireValue())
			.ToArray();
		projectedPortNumbers[0].Should().Equal(projectedPortNumbers[1]);
		projectedPortNumbers[0][0].Should().BeOneOf((byte)24, (byte)25);
		this.concurrentProjections.Should().AllSatisfy(projection =>
			projection.Get(NodeManagerParameterCatalogue.AgentType).ToWireValue().Should().Equal([12]));
	}

	[When(@"two NodeManager stores change existing Non-Volatile Parameter 1 concurrently")]
	public async Task WhenTwoNodeManagerStoresChangeExistingNonVolatileParameterConcurrently()
	{
		await this.StoreAsync(ParameterValue.FromWireValue([25]));

		using var barrier = new Barrier(2);
		this.parameterSetUpdateBarrier = barrier;
		try
		{
			this.concurrentWrites = await Task.WhenAll(
				this.TryStoreAsync(ParameterValue.FromWireValue([24])),
				this.TryStoreAsync(ParameterValue.FromWireValue([23])));
		}
		finally
		{
			this.parameterSetUpdateBarrier = null;
		}
	}

	[Then(@"one NodeManager store reports a concurrent Parameter Store update")]
	public void ThenOneNodeManagerStoreReportsAConcurrentParameterStoreUpdate()
	{
		this.concurrentWrites.Should().NotBeNull().And.HaveCount(2);
		this.concurrentWrites!.Count(write => write.Succeeded).Should().Be(1);
		this.concurrentWrites.Count(write => write.ReportsConcurrencyConflict).Should().Be(1);
	}

	[AfterScenario]
	public async Task DisposeDatabaseAsync()
	{
		if (this.database is not null)
		{
			await this.database.DisposeAsync();
		}
	}

	private async Task<NodeManagerCurrentParameterProjection> LoadProjectionAsync(
		CommunicationsAddress localAddress)
	{
		await using var context = this.CreateContext();
		var bootstrapper = new NodeManagerParameterBootstrapper(
			new EfNodeManagerParameterStore(context));

		return await bootstrapper.LoadCurrentParameterProjectionAsync(
			NodeManagerParameterBootstrapConfiguration.FromAddress(localAddress));
	}

	private NodeManagerDbContext CreateContext()
	{
		var optionsBuilder = new DbContextOptionsBuilder<NodeManagerDbContext>()
			.UseNpgsql(this.database!.GetConnectionString());
		if (this.parameterSetUpdateBarrier is not null)
		{
			optionsBuilder.AddInterceptors(
				new ParameterSetUpdateBarrierInterceptor(this.parameterSetUpdateBarrier));
		}

		return new NodeManagerDbContext(optionsBuilder.Options);
	}

	private async Task StoreAsync(ParameterValue parameterValue)
	{
		await this.StoreAsync(ParameterTable.NonVolatile, parameterValue);
	}

	private async Task StoreAsync(ParameterTable parameterTable, byte value)
	{
		await this.StoreAsync(parameterTable, ParameterValue.FromWireValue([value]));
	}

	private async Task StoreAsync(ParameterTable parameterTable, ParameterValue parameterValue)
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfNodeManagerParameterStore(context);
		await store.StoreAsync(
			parameterTable,
			ParameterNumber.FromValue(1),
			parameterValue);
	}

	private async Task<ParameterStoreWriteResult> TryStoreAsync(ParameterValue parameterValue)
	{
		try
		{
			await this.StoreAsync(parameterValue);
			return new ParameterStoreWriteResult(true, false);
		}
		catch (DbUpdateConcurrencyException)
		{
			return new ParameterStoreWriteResult(false, true);
		}
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed record ParameterStoreWriteResult(
		bool Succeeded,
		bool ReportsConcurrencyConflict);

	private sealed class ParameterSetUpdateBarrierInterceptor(Barrier barrier) : DbCommandInterceptor
	{
		public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
			DbCommand command,
			CommandEventData eventData,
			InterceptionResult<DbDataReader> result,
			CancellationToken cancellationToken = default)
		{
			if (command.CommandText.Contains("UPDATE", StringComparison.OrdinalIgnoreCase) &&
				command.CommandText.Contains("parameter_set", StringComparison.OrdinalIgnoreCase))
			{
				barrier.SignalAndWait(cancellationToken);
			}

			return ValueTask.FromResult(result);
		}
	}
}
