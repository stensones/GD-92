using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Testcontainers.PostgreSql;

namespace Router.Persistence.Tests.Integration;

[Binding]
public sealed class RouterPasswordVerifierPersistenceSteps
{
	private static readonly ParameterNumber PasswordParameterNumber = ParameterNumber.FromValue(5);

	private PostgreSqlContainer? database;
	private PasswordValue? originalPassword;
	private PasswordValue? differentPassword;
	private PasswordVerifier? reloadedVerifier;

	[Given(@"an empty isolated Router password verifier database")]
	public async Task GivenAnEmptyIsolatedRouterPasswordVerifierDatabase()
	{
		this.database = new PostgreSqlBuilder("postgres:17.5").Build();
		await this.database.StartAsync();

		await using var context = this.CreateContext();
		await context.Database.MigrateAsync();
	}

	[Given(@"a persistent Router permanent Parameter Set")]
	public async Task GivenAPersistentRouterPermanentParameterSet()
	{
		await using var context = this.CreateContext();
		var parameterStore = new EfRouterParameterStore(context);

		await parameterStore.StoreAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(1),
			ParameterValue.FromWireValue([26]));
	}

	[When(@"I store a Level 1 password verifier in Permanent Parameter Table at Parameter 5")]
	public async Task WhenIStoreALevel1PasswordVerifier()
	{
		this.originalPassword = CreatePassword();
		this.differentPassword = CreateDifferentPassword(this.originalPassword.Value);
		var verifier = PasswordVerifier.Create(
			this.originalPassword.Value,
			PasswordVerifierWorkFactor.Default);

		await using var context = this.CreateContext();
		IRouterPasswordVerifierStore store = new EfRouterPasswordVerifierStore(context);
		await store.StoreAsync(
			ParameterTable.Permanent,
			PasswordParameterNumber,
			verifier);
	}

	[Then(@"a fresh Router password verifier store retrieves a verifier that accepts the original password")]
	public async Task ThenAFreshStoreRetrievesTheOriginalPasswordVerifier()
	{
		await using var context = this.CreateContext();
		IRouterPasswordVerifierStore store = new EfRouterPasswordVerifierStore(context);

		this.reloadedVerifier = await store.GetAsync(
			ParameterTable.Permanent,
			PasswordParameterNumber);

		this.reloadedVerifier.Should().NotBeNull();
		this.reloadedVerifier!.Verifies(this.originalPassword!.Value).Should().BeTrue();
	}

	[Then(@"it rejects a different password")]
	public void ThenItRejectsADifferentPassword()
	{
		this.reloadedVerifier.Should().NotBeNull();
		this.reloadedVerifier!.Verifies(this.differentPassword!.Value).Should().BeFalse();
	}

	[AfterScenario]
	public async Task DisposeDatabaseAsync()
	{
		if (this.database is not null)
		{
			await this.database.DisposeAsync();
		}
	}

	private RouterDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(this.database!.GetConnectionString())
			.Options;

		return new RouterDbContext(options);
	}

	private static PasswordValue CreatePassword()
	{
		return PasswordValue.FromValue(
			SevenBitAsciiString.FromValue(Guid.NewGuid().ToString("N")[..10]));
	}

	private static PasswordValue CreateDifferentPassword(PasswordValue password)
	{
		var value = password.Value.Value;
		var firstCharacter = value[0] == '0' ? '1' : '0';

		return PasswordValue.FromValue(
			SevenBitAsciiString.FromValue($"{firstCharacter}{value[1..]}"));
	}
}
