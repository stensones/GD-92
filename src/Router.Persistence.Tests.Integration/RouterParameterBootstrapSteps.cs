using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Router.Persistence;
using Stensones.GD92.Fields;
using Testcontainers.PostgreSql;

namespace Router.Persistence.Tests.Integration;

[Binding]
public sealed class RouterParameterBootstrapSteps
{
	private PostgreSqlContainer? database;
	private RouterCurrentParameterProjection? currentParameters;

	[Given(@"an empty isolated Router database")]
	public async Task GivenAnEmptyIsolatedRouterDatabase()
	{
		this.database = new PostgreSqlBuilder("postgres:17.5").Build();
		await this.database.StartAsync();

		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(this.database.GetConnectionString())
			.Options;
		await using var context = new RouterDbContext(options);
		await context.Database.MigrateAsync();
	}

	[When(@"Router Parameter startup loads brigade or agency number (.*)")]
	public Task WhenRouterParameterStartupLoadsBrigadeOrAgencyNumber(byte brigadeOrAgencyNumber)
	{
		return this.LoadCurrentParametersAsync(
			BrigadeOrAgencyIdentifier.FromValue(brigadeOrAgencyNumber));
	}

	[Then(@"its current Parameter 1 is brigade or agency number (.*)")]
	public void ThenItsCurrentParameterIsBrigadeOrAgencyNumber(byte brigadeOrAgencyNumber)
	{
		this.currentParameters.Should().NotBeNull();
		this.currentParameters!.BrigadeOrAgencyIdentifier.ToWireValue()
			.Should().Equal([brigadeOrAgencyNumber]);
	}

	[When(@"Router Parameter startup runs again")]
	public Task WhenRouterParameterStartupRunsAgain()
	{
		return this.LoadCurrentParametersAsync(BrigadeOrAgencyIdentifier.FromValue(26));
	}

	[AfterScenario]
	public async Task DisposeDatabaseAsync()
	{
		if (this.database is not null)
		{
			await this.database.DisposeAsync();
		}
	}

	private async Task LoadCurrentParametersAsync(BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier)
	{
		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(this.database!.GetConnectionString())
			.Options;
		await using var context = new RouterDbContext(options);
		var store = new EfRouterParameterStore(context);
		var bootstrapper = new RouterParameterBootstrapper(store);

		this.currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(
			brigadeOrAgencyIdentifier);
	}
}
