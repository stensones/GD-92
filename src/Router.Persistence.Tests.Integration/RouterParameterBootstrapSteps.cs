using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ParticipantParameters;
using Reqnroll;
using Router.Persistence;
using Stensones.GD92.Fields;
using Testcontainers.PostgreSql;

namespace Router.Persistence.Tests.Integration;

[Binding]
public sealed class RouterParameterBootstrapSteps
{
	private static readonly PasswordValue InitialLevel1Password =
		PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));

	private PostgreSqlContainer? database;
	private RouterCurrentParameterProjection? currentParameters;
	private CommunicationsAddress? localAddress;

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

	[When(@"Router Parameter startup bootstraps its initial Parameters")]
	public Task WhenRouterParameterStartupBootstrapsItsInitialParameters()
	{
		return this.LoadCurrentParametersAsync(BrigadeOrAgencyIdentifier.FromValue(26));
	}

	[Then(@"its current Password is the neutral local Router Password Parameter")]
	public void ThenItsCurrentPasswordIsTheNeutralLocalRouterPasswordParameter()
	{
		this.currentParameters.Should().NotBeNull();
		this.localAddress.Should().NotBeNull();
		this.currentParameters!.CurrentPassword.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated));
		this.currentParameters.CurrentPassword.Password.ToWireValue().Should().Equal([0]);
		this.currentParameters.CurrentPassword.CommunicationsAddress.Should().Be(this.localAddress);
	}

	[Then(@"its current No Acknowledgement Timeout is (.*) seconds")]
	public void ThenItsCurrentNoAcknowledgementTimeoutIsSeconds(int seconds)
	{
		this.currentParameters.Should().NotBeNull();
		this.currentParameters!.NoAcknowledgementTimeout.Should().Be(
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(checked((byte)seconds))));
	}

	[Then(@"its current Retries value is (.*)")]
	public void ThenItsCurrentRetriesValueIs(int retries)
	{
		this.currentParameters.Should().NotBeNull();
		this.currentParameters!.Retries.Should().Be(
			Retries.FromValue(Word8.FromValue(checked((byte)retries))));
	}

	[Then(@"its Level 1 Password has permanent and non-volatile verifiers but no opaque Parameter values")]
	public async Task ThenItsLevel1PasswordHasPermanentAndNonVolatileVerifiersButNoOpaqueParameterValues()
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore parameterStore = new EfRouterParameterStore(context);
		IRouterLevel1PasswordVerifierStore passwordVerifierStore =
			new EfRouterPasswordVerifierStore(context);
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Level1PasswordNumber))
			.Should().BeNull();
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Level1PasswordNumber))
			.Should().BeNull();
		(await passwordVerifierStore.GetAsync(ParameterTable.Permanent))!
			.Verifies(InitialLevel1Password).Should().BeTrue();
		(await passwordVerifierStore.GetAsync(ParameterTable.NonVolatile))!
			.Verifies(InitialLevel1Password).Should().BeTrue();
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
		this.localAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(brigadeOrAgencyIdentifier),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(0)));
		await using var context = this.CreateContext();
		var store = new EfRouterParameterStore(context);
		var passwordVerifierStore = new EfRouterPasswordVerifierStore(context);
		var bootstrapper = new RouterParameterBootstrapper(store, passwordVerifierStore);

		this.currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(
			RouterParameterBootstrapConfiguration.FromValues(
				this.localAddress,
				InitialLevel1Password,
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
				Retries.FromValue(Word8.FromValue(3))));
	}

	private RouterDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(this.database!.GetConnectionString())
			.Options;

		return new RouterDbContext(options);
	}
}
