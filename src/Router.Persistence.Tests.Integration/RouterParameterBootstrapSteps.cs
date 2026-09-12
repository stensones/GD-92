using AwesomeAssertions;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParticipantParameters;
using Reqnroll;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Testcontainers.PostgreSql;

namespace Router.Persistence.Tests.Integration;

[Binding]
public sealed class RouterParameterBootstrapSteps
{
	private static readonly PasswordValue InitialLevel1Password =
		PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));

	private PostgreSqlContainer? database;
	private RouterCurrentParameterProjection? currentParameters;
	private RouterCurrentParameterProjection[]? concurrentProjections;
	private ParameterStoreWriteResult[]? concurrentWrites;
	private Barrier? parameterSetUpdateBarrier;
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

	[When(@"Router stores Non-Volatile Parameter 1 with value (.*)")]
	public async Task WhenRouterStoresNonVolatileParameterWithValue(byte value)
	{
		await this.StoreAsync(ParameterTable.NonVolatile, value);
	}

	[When(@"Router stores Permanent Parameter 1 with value (.*)")]
	public async Task WhenRouterStoresPermanentParameterWithValue(byte value)
	{
		await this.StoreAsync(ParameterTable.Permanent, value);
	}

	[Then(@"its Permanent Parameter 1 has value (.*)")]
	public async Task ThenItsPermanentParameterHasValue(byte value)
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfRouterParameterStore(context);

		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([value]);
	}

	[Then(@"its Permanent Parameter 1 is absent")]
	public async Task ThenItsPermanentParameterIsAbsent()
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfRouterParameterStore(context);

		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(1)))
			.Should().BeNull();
	}

	[Then(@"its Non-Volatile Parameter 1 has value (.*)")]
	public async Task ThenItsNonVolatileParameterHasValue(byte value)
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfRouterParameterStore(context);

		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([value]);
	}

	[Then(@"its Non-Volatile Parameter 1 is absent")]
	public async Task ThenItsNonVolatileParameterIsAbsent()
	{
		await using var context = this.CreateContext();
		IParticipantParameterStore store = new EfRouterParameterStore(context);

		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(1)))
			.Should().BeNull();
	}

	[When(@"two Router Parameter starts use different bootstrap configurations concurrently")]
	public async Task WhenTwoRouterParameterStartsUseDifferentBootstrapConfigurationsConcurrently()
	{
		this.concurrentProjections = await Task.WhenAll(
			this.LoadCurrentParametersAsync(
				CreateAddress(26, 100, 0),
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
				Retries.FromValue(Word8.FromValue(3))),
			this.LoadCurrentParametersAsync(
				CreateAddress(42, 200, 0),
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER")),
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(8)),
				Retries.FromValue(Word8.FromValue(7))));
	}

	[Then(@"both Router starts project one complete first-committed Parameter set")]
	public void ThenBothRouterStartsProjectOneCompleteFirstCommittedParameterSet()
	{
		this.concurrentProjections.Should().NotBeNull().And.HaveCount(2);

		var first = this.concurrentProjections![0];
		var second = this.concurrentProjections[1];
		first.BrigadeOrAgencyIdentifier.ToWireValue()
			.Should().Equal(second.BrigadeOrAgencyIdentifier.ToWireValue());
		first.CurrentPassword.ToWireValue().Should().Equal(second.CurrentPassword.ToWireValue());
		first.NoAcknowledgementTimeout.Should().Be(second.NoAcknowledgementTimeout);
		first.Retries.Should().Be(second.Retries);
		first.BrigadeOrAgencyIdentifier.ToWireValue()[0].Should().BeOneOf((byte)26, (byte)42);
	}

	[Then(@"both Router starts use the first-committed Level 1 password verifier")]
	public void ThenBothRouterStartsUseTheFirstCommittedLevelPasswordVerifier()
	{
		this.concurrentProjections.Should().NotBeNull().And.HaveCount(2);

		var initialFirePassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var initialWaterPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"));
		var first = this.concurrentProjections![0].Level1PasswordVerifier;
		var second = this.concurrentProjections[1].Level1PasswordVerifier;
		first.Verifies(initialFirePassword).Should().Be(second.Verifies(initialFirePassword));
		first.Verifies(initialWaterPassword).Should().Be(second.Verifies(initialWaterPassword));
		(first.Verifies(initialFirePassword) || first.Verifies(initialWaterPassword)).Should().BeTrue();
	}

	[When(@"two Router stores change existing Non-Volatile Parameter 1 concurrently")]
	public async Task WhenTwoRouterStoresChangeExistingNonVolatileParameterConcurrently()
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

	[Then(@"one Router store reports a concurrent Parameter Store update")]
	public void ThenOneRouterStoreReportsAConcurrentParameterStoreUpdate()
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

	private async Task LoadCurrentParametersAsync(BrigadeOrAgencyIdentifier brigadeOrAgencyIdentifier)
	{
		this.localAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(brigadeOrAgencyIdentifier),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(0)));
		this.currentParameters = await this.LoadCurrentParametersAsync(
			this.localAddress,
			InitialLevel1Password,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)));
	}

	private async Task<RouterCurrentParameterProjection> LoadCurrentParametersAsync(
		CommunicationsAddress localAddress,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		await using var context = this.CreateContext();
		var bootstrapper = new RouterParameterBootstrapper(
			new EfRouterParameterStore(context),
			new EfRouterPasswordVerifierStore(context));

		return await bootstrapper.LoadCurrentParameterProjectionAsync(
			RouterParameterBootstrapConfiguration.FromValues(
				localAddress,
				NodeName.FromValue(SevenBitAsciiString.FromValue("Station End")),
				MaximumMessageLength.FromValue(1_023),
				CreateAddress(26, 100, 25),
				CreateAddress(26, 100, 25),
				initialLevel1Password,
				noAcknowledgementTimeout,
				retries,
				ManualAcknowledgementTimeout.FromValue(60),
				TimeAndDate.FromValue(SevenBitAsciiString.FromValue("07SEP26154309")),
				MdtTable.FromEntries()));
	}

	private RouterDbContext CreateContext()
	{
		var optionsBuilder = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(this.database!.GetConnectionString());
		if (this.parameterSetUpdateBarrier is not null)
		{
			optionsBuilder.AddInterceptors(
				new ParameterSetUpdateBarrierInterceptor(this.parameterSetUpdateBarrier));
		}

		return new RouterDbContext(optionsBuilder.Options);
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
		IParticipantParameterStore store = new EfRouterParameterStore(context);
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
