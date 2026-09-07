using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NodeManager.Persistence;
using Reqnroll;
using Router.Persistence;
using Stensones.GD92.Fields;
using Testcontainers.PostgreSql;

namespace StationEnd.Tests.Integration;

[Binding]
public sealed class ParticipantParameterStoreInitializationSteps
{
	private PostgreSqlContainer? nodeManagerDatabase;
	private NodeManagerCurrentParameterProjection[]? nodeManagerProjections;
	private PostgreSqlContainer? routerDatabase;
	private RouterCurrentParameterProjection[]? routerProjections;

	[Given(@"empty Router and NodeManager Participant Parameter Stores")]
	public async Task GivenEmptyRouterAndNodeManagerParticipantParameterStores()
	{
		this.routerDatabase = new PostgreSqlBuilder("postgres:17.5").Build();
		this.nodeManagerDatabase = new PostgreSqlBuilder("postgres:17.5").Build();

		await Task.WhenAll(this.routerDatabase.StartAsync(), this.nodeManagerDatabase.StartAsync());
		await Task.WhenAll(this.MigrateRouterDatabaseAsync(), this.MigrateNodeManagerDatabaseAsync());
	}

	[When(@"two Router starts use different bootstrap configurations concurrently")]
	public async Task WhenTwoRouterStartsUseDifferentBootstrapConfigurationsConcurrently()
	{
		this.routerProjections = await Task.WhenAll(
			this.LoadRouterProjectionAsync(
				CreateAddress(26, 100, 0),
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
				Retries.FromValue(Word8.FromValue(3))),
			this.LoadRouterProjectionAsync(
				CreateAddress(42, 200, 0),
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER")),
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(8)),
				Retries.FromValue(Word8.FromValue(7))));
	}

	[When(@"two NodeManager starts use different bootstrap configurations concurrently")]
	public async Task WhenTwoNodeManagerStartsUseDifferentBootstrapConfigurationsConcurrently()
	{
		this.nodeManagerProjections = await Task.WhenAll(
			this.LoadNodeManagerProjectionAsync(CreateAddress(26, 100, 25)),
			this.LoadNodeManagerProjectionAsync(CreateAddress(26, 100, 24)));
	}

	[Then(@"each Router Current Parameter Table contains one complete first-committed Router Parameter set")]
	public void ThenEachRouterCurrentParameterTableContainsOneCompleteFirstCommittedRouterParameterSet()
	{
		this.routerProjections.Should().NotBeNull().And.HaveCount(2);

		var first = this.routerProjections![0];
		var second = this.routerProjections[1];
		first.BrigadeOrAgencyIdentifier.ToWireValue()
			.Should().Equal(second.BrigadeOrAgencyIdentifier.ToWireValue());
		first.CurrentPassword.ToWireValue().Should().Equal(second.CurrentPassword.ToWireValue());
		first.NoAcknowledgementTimeout.Should().Be(second.NoAcknowledgementTimeout);
		first.Retries.Should().Be(second.Retries);
		first.BrigadeOrAgencyIdentifier.ToWireValue()[0].Should().BeOneOf((byte)26, (byte)42);
	}

	[Then(@"each NodeManager Current Parameter Table contains one complete first-committed NodeManager Parameter set")]
	public void ThenEachNodeManagerCurrentParameterTableContainsOneCompleteFirstCommittedNodeManagerParameterSet()
	{
		this.nodeManagerProjections.Should().NotBeNull().And.HaveCount(2);

		var projectedPortNumbers = this.nodeManagerProjections!
			.Select(projection => projection.Get(NodeManagerParameterCatalogue.PortNumber).ToWireValue())
			.ToArray();
		projectedPortNumbers[0].Should().Equal(projectedPortNumbers[1]);
		projectedPortNumbers[0][0].Should().BeOneOf((byte)24, (byte)25);
		this.nodeManagerProjections.Should().AllSatisfy(projection =>
			projection.Get(NodeManagerParameterCatalogue.AgentType).ToWireValue().Should().Equal([12]));
	}

	[Then(@"each Router Current Parameter Table uses the first-committed Level 1 password verifier")]
	public void ThenEachRouterCurrentParameterTableUsesTheFirstCommittedLevelPasswordVerifier()
	{
		this.routerProjections.Should().NotBeNull().And.HaveCount(2);

		var initialFirePassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var initialWaterPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"));
		var first = this.routerProjections![0].Level1PasswordVerifier;
		var second = this.routerProjections[1].Level1PasswordVerifier;
		first.Verifies(initialFirePassword).Should().Be(second.Verifies(initialFirePassword));
		first.Verifies(initialWaterPassword).Should().Be(second.Verifies(initialWaterPassword));
		(first.Verifies(initialFirePassword) || first.Verifies(initialWaterPassword)).Should().BeTrue();
	}

	[AfterScenario]
	public async Task DisposeDatabasesAsync()
	{
		var disposalTasks = new List<Task>();
		if (this.routerDatabase is not null)
		{
			disposalTasks.Add(this.routerDatabase.DisposeAsync().AsTask());
		}

		if (this.nodeManagerDatabase is not null)
		{
			disposalTasks.Add(this.nodeManagerDatabase.DisposeAsync().AsTask());
		}

		await Task.WhenAll(disposalTasks);
	}

	private async Task<RouterCurrentParameterProjection> LoadRouterProjectionAsync(
		CommunicationsAddress localAddress,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		await using var context = new RouterDbContext(
			new DbContextOptionsBuilder<RouterDbContext>()
				.UseNpgsql(this.routerDatabase!.GetConnectionString())
				.Options);
		var bootstrapper = new RouterParameterBootstrapper(
			new EfRouterParameterStore(context),
			new EfRouterPasswordVerifierStore(context));

		return await bootstrapper.LoadCurrentParameterProjectionAsync(
			RouterParameterBootstrapConfiguration.FromValues(
				localAddress,
				initialLevel1Password,
				noAcknowledgementTimeout,
				retries));
	}

	private async Task<NodeManagerCurrentParameterProjection> LoadNodeManagerProjectionAsync(
		CommunicationsAddress localAddress)
	{
		await using var context = new NodeManagerDbContext(
			new DbContextOptionsBuilder<NodeManagerDbContext>()
				.UseNpgsql(this.nodeManagerDatabase!.GetConnectionString())
				.Options);
		var bootstrapper = new NodeManagerParameterBootstrapper(
			new EfNodeManagerParameterStore(context));

		return await bootstrapper.LoadCurrentParameterProjectionAsync(
			NodeManagerParameterBootstrapConfiguration.FromAddress(localAddress));
	}

	private async Task MigrateRouterDatabaseAsync()
	{
		await using var context = new RouterDbContext(
			new DbContextOptionsBuilder<RouterDbContext>()
				.UseNpgsql(this.routerDatabase!.GetConnectionString())
				.Options);
		await context.Database.MigrateAsync();
	}

	private async Task MigrateNodeManagerDatabaseAsync()
	{
		await using var context = new NodeManagerDbContext(
			new DbContextOptionsBuilder<NodeManagerDbContext>()
				.UseNpgsql(this.nodeManagerDatabase!.GetConnectionString())
				.Options);
		await context.Database.MigrateAsync();
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
