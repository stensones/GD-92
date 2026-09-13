using AwesomeAssertions;
using ParticipantParameters;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence.Tests.Unit;

public sealed class RouterParameterBootstrapperTests
{
	[Fact]
	public async Task Seeds_permanent_and_non_volatile_router_parameter_one_when_the_store_is_empty()
	{
		var store = new InMemoryRouterParameterStore();
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var bootstrapper = new RouterParameterBootstrapper(
			store,
			new InMemoryRouterPasswordVerifierStore());

		var currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(
			CreateBootstrapConfiguration(initialPassword));
		var permanentValue = await store.GetAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(1));
		var nonVolatileValue = await store.GetAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(1));

		currentParameters.BrigadeOrAgencyIdentifier.ToWireValue().Should().Equal([26]);
		permanentValue.Should().NotBeNull();
		permanentValue!.ToWireValue().Should().Equal([26]);
		nonVolatileValue.Should().NotBeNull();
		nonVolatileValue!.ToWireValue().Should().Equal([26]);
	}

	[Fact]
	public async Task Seeds_each_password_parameter_with_its_own_initial_password()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var passwordVerifierStore = new InMemoryRouterPasswordVerifierStore();
		var initialPasswords = Enumerable.Range(0, 4)
			.Select(_ => PasswordValue.FromValue(
				SevenBitAsciiString.FromValue(Guid.NewGuid().ToString("N")[..10])))
			.ToArray();
		initialPasswords.Should().OnlyHaveUniqueItems();
		var configuration = RouterParameterBootstrapConfiguration.FromValues(
			CreateAddress(26, 100, 0),
			CreateNodeName(),
			MaximumMessageLength.FromValue(1_023),
			CreateAddress(26, 100, 25),
			CreateAddress(26, 100, 25),
			initialPasswords[0],
			initialPasswords[1],
			initialPasswords[2],
			initialPasswords[3],
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)),
			ManualAcknowledgementTimeout.FromValue(60),
			CreateInstallationTime(),
			MdtTable.FromEntries());

		var currentParameters = await new RouterParameterBootstrapper(
			parameterStore,
			passwordVerifierStore).LoadCurrentParameterProjectionAsync(configuration);

		var currentVerifiers = new[]
		{
			currentParameters.Level1PasswordVerifier,
			currentParameters.Level2PasswordVerifier,
			currentParameters.Level3PasswordVerifier,
			currentParameters.Level4PasswordVerifier
		};
		foreach (var (number, password, verifier) in RouterParameterCatalogue.PasswordNumbers
			.Zip(initialPasswords, currentVerifiers))
		{
			(await passwordVerifierStore.GetAsync(ParameterTable.Permanent, number))!
				.Verifies(password).Should().BeTrue();
			(await passwordVerifierStore.GetAsync(ParameterTable.NonVolatile, number))!
				.Verifies(password).Should().BeTrue();
			verifier.Verifies(password).Should().BeTrue();
		}
	}

	[Fact]
	public async Task Seeds_the_initial_typed_Router_Parameters_and_projects_their_non_volatile_values()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var passwordVerifierStore = new InMemoryRouterPasswordVerifierStore();
		var localAddress = CreateAddress(26, 100, 0);
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var configuration = RouterParameterBootstrapConfiguration.FromValues(
			localAddress,
			CreateNodeName(),
			MaximumMessageLength.FromValue(1_023),
			CreateAddress(26, 100, 25),
			CreateAddress(26, 100, 25),
			initialPassword,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)),
			ManualAcknowledgementTimeout.FromValue(60),
			CreateInstallationTime(),
			MdtTable.FromEntries());
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			passwordVerifierStore);

		var currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(configuration);
		var permanentNodeNumber = await parameterStore.GetAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(2));
		var nonVolatileNodeNumber = await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(2));
		var permanentNodeName = await parameterStore.GetAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(3));
		var nonVolatileNodeName = await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(3));

		currentParameters.BrigadeOrAgencyIdentifier.ToWireValue().Should().Equal([26]);
		permanentNodeNumber.Should().NotBeNull();
		permanentNodeNumber!.ToWireValue().Should().Equal([0, 100]);
		nonVolatileNodeNumber.Should().NotBeNull();
		nonVolatileNodeNumber!.ToWireValue().Should().Equal([0, 100]);
		permanentNodeName.Should().NotBeNull();
		permanentNodeName!.ToWireValue().Should().Equal([
			11, (byte)'S', (byte)'t', (byte)'a', (byte)'t', (byte)'i',
			(byte)'o', (byte)'n', (byte)' ', (byte)'E', (byte)'n', (byte)'d'
		]);
		nonVolatileNodeName.Should().NotBeNull();
		nonVolatileNodeName!.ToWireValue().Should().Equal([
			11, (byte)'S', (byte)'t', (byte)'a', (byte)'t', (byte)'i',
			(byte)'o', (byte)'n', (byte)' ', (byte)'E', (byte)'n', (byte)'d'
		]);
		currentParameters.CurrentPassword.ToWireValue().Should().Equal(
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
				Password.FromValue(PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress).ToWireValue());
		currentParameters.NoAcknowledgementTimeout.Should().Be(
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)));
		currentParameters.Retries.Should().Be(Retries.FromValue(Word8.FromValue(3)));
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.CurrentPassword.Number))!
			.ToWireValue().Should().Equal(currentParameters.CurrentPassword.ToWireValue());
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.CurrentPassword.Number))!
			.ToWireValue().Should().Equal(currentParameters.CurrentPassword.ToWireValue());
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Number))!
			.ToWireValue().Should().Equal([5]);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Number))!
			.ToWireValue().Should().Equal([5]);
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Retries.Number))!
			.ToWireValue().Should().Equal([3]);
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Retries.Number))!
			.ToWireValue().Should().Equal([3]);
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Level1PasswordNumber))
			.Should().BeNull();
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Level1PasswordNumber))
			.Should().BeNull();
		(await passwordVerifierStore.GetAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Level1PasswordNumber))!
			.Verifies(initialPassword).Should().BeTrue();
		(await passwordVerifierStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Level1PasswordNumber))!
			.Verifies(initialPassword).Should().BeTrue();
		currentParameters.Level1PasswordVerifier.Verifies(initialPassword).Should().BeTrue();
	}

	[Fact]
	public async Task Seeds_permanent_and_non_volatile_router_parameters_nine_to_eleven_from_its_configuration()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var networkManagerAddress = CreateAddress(26, 100, 25);
		var configuration = RouterParameterBootstrapConfiguration.FromValues(
			CreateAddress(26, 100, 0),
			CreateNodeName(),
			MaximumMessageLength.FromValue(1_023),
			networkManagerAddress,
			networkManagerAddress,
			initialPassword,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)),
			ManualAcknowledgementTimeout.FromValue(60),
			CreateInstallationTime(),
			MdtTable.FromEntries());
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			new InMemoryRouterPasswordVerifierStore());

		await bootstrapper.LoadCurrentParameterProjectionAsync(configuration);

		await AssertStoredParameterValueAsync(
			parameterStore,
			ParameterTable.Permanent,
			9,
			[3, 255]);
		await AssertStoredParameterValueAsync(
			parameterStore,
			ParameterTable.NonVolatile,
			9,
			[3, 255]);
		await AssertStoredParameterValueAsync(
			parameterStore,
			ParameterTable.Permanent,
			10,
			[26, 25, 25]);
		await AssertStoredParameterValueAsync(
			parameterStore,
			ParameterTable.NonVolatile,
			10,
			[26, 25, 25]);
		await AssertStoredParameterValueAsync(
			parameterStore,
			ParameterTable.Permanent,
			11,
			[26, 25, 25]);
		await AssertStoredParameterValueAsync(
			parameterStore,
			ParameterTable.NonVolatile,
			11,
			[26, 25, 25]);
	}

	[Fact]
	public async Task Seeds_empty_router_tables_as_canonical_parameter_values_in_both_persistent_tables()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			new InMemoryRouterPasswordVerifierStore());

		await bootstrapper.LoadCurrentParameterProjectionAsync(
			CreateBootstrapConfiguration(initialPassword));

		await AssertEmptyStoredParameterValueAsync(
			parameterStore,
			RouterParameterCatalogue.RouterTable.Number,
			RouterParameterCatalogue.RouterTable.Encode(RoutingTable.FromEntries()));
		await AssertEmptyStoredParameterValueAsync(
			parameterStore,
			RouterParameterCatalogue.PstnTable.Number,
			RouterParameterCatalogue.PstnTable.Encode(PstnTable.FromEntries()));
		await AssertEmptyStoredParameterValueAsync(
			parameterStore,
			RouterParameterCatalogue.WanTable.Number,
			RouterParameterCatalogue.WanTable.Encode(WanTable.FromEntries()));
		await AssertEmptyStoredParameterValueAsync(
			parameterStore,
			RouterParameterCatalogue.LanTable.Number,
			RouterParameterCatalogue.LanTable.Encode(LanTable.FromEntries()));
		await AssertEmptyStoredParameterValueAsync(
			parameterStore,
			RouterParameterCatalogue.IsdnTable.Number,
			RouterParameterCatalogue.IsdnTable.Encode(IsdnTable.FromEntries()));
	}

	[Fact]
	public async Task Seeds_manual_acknowledgement_timeout_installation_time_and_empty_MDT_table_in_both_persistent_tables()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var installationTime = TimeAndDate.FromValue(
			SevenBitAsciiString.FromValue("07SEP26154309"));
		var configuration = RouterParameterBootstrapConfiguration.FromValues(
			CreateAddress(26, 100, 0),
			CreateNodeName(),
			MaximumMessageLength.FromValue(1_023),
			CreateAddress(26, 100, 25),
			CreateAddress(26, 100, 25),
			initialPassword,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)),
			ManualAcknowledgementTimeout.FromValue(60),
			installationTime,
			MdtTable.FromEntries());
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			new InMemoryRouterPasswordVerifierStore());

		await bootstrapper.LoadCurrentParameterProjectionAsync(configuration);

		foreach (var parameterTable in new[] { ParameterTable.Permanent, ParameterTable.NonVolatile })
		{
			await AssertStoredParameterValueAsync(parameterStore, parameterTable, 18, [0, 60]);
			await AssertStoredParameterValueAsync(
				parameterStore,
				parameterTable,
				20,
				Convert.FromHexString("30375345503236313534333039"));
			await AssertStoredParameterValueAsync(parameterStore, parameterTable, 21, []);
		}
	}

	[Fact]
	public async Task Fails_startup_when_the_non_volatile_Level1_password_verifier_is_missing()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var passwordVerifierStore = new InMemoryRouterPasswordVerifierStore();
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		await passwordVerifierStore.StoreAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Level1PasswordNumber,
			PasswordVerifier.Create(initialPassword, PasswordVerifierWorkFactor.Default));
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			passwordVerifierStore);

		var load = async () => await bootstrapper.LoadCurrentParameterProjectionAsync(
			CreateBootstrapConfiguration(initialPassword));

		await load.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Non-Volatile Parameter 5 password verifier is missing*");
		(await passwordVerifierStore.GetAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Level1PasswordNumber))
			.Should().BeNull();
	}

	[Fact]
	public async Task Fails_startup_with_a_clear_error_when_the_non_volatile_Level1_password_verifier_is_malformed()
	{
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var bootstrapper = new RouterParameterBootstrapper(
			new InMemoryRouterParameterStore(),
			new MalformedNonVolatilePasswordVerifierStore(
				PasswordVerifier.Create(initialPassword, PasswordVerifierWorkFactor.Default)));

		var load = async () => await bootstrapper.LoadCurrentParameterProjectionAsync(
			CreateBootstrapConfiguration(initialPassword));

		await load.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Non-Volatile*Parameter 5 password verifier is malformed*");
	}

	[Fact]
	public async Task Projects_each_current_value_from_its_non_volatile_value()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var passwordVerifierStore = new InMemoryRouterPasswordVerifierStore();
		var permanentPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("PERMANENT"));
		var nonVolatilePassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("CURRENT"));
		var permanentAddress = CreateAddress(26, 100, 0);
		var nonVolatileAddress = CreateAddress(42, 100, 25);
		await parameterStore.StoreAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(1),
			ParameterValue.FromWireValue([26]));
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(1),
			ParameterValue.FromWireValue([42]));
		await parameterStore.StoreAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(4),
			CreateNeutralCurrentPassword(permanentAddress));
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(4),
			CreateNeutralCurrentPassword(nonVolatileAddress));
		await parameterStore.StoreAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(12),
			ParameterValue.FromWireValue([5]));
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(12),
			ParameterValue.FromWireValue([8]));
		await parameterStore.StoreAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(19),
			ParameterValue.FromWireValue([3]));
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(19),
			ParameterValue.FromWireValue([7]));
		await passwordVerifierStore.StoreAsync(
			ParameterTable.Permanent,
			RouterParameterCatalogue.Level1PasswordNumber,
			PasswordVerifier.Create(permanentPassword, PasswordVerifierWorkFactor.Default));
		await passwordVerifierStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.Level1PasswordNumber,
			PasswordVerifier.Create(nonVolatilePassword, PasswordVerifierWorkFactor.Default));
		var bootstrapper = new RouterParameterBootstrapper(parameterStore, passwordVerifierStore);

		var currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(
			CreateBootstrapConfiguration(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("INITIAL"))));

		currentParameters.BrigadeOrAgencyIdentifier.ToWireValue().Should().Equal([42]);
		currentParameters.CurrentPassword.ToWireValue().Should().Equal(
			CreateNeutralCurrentPassword(nonVolatileAddress).ToWireValue());
		currentParameters.NoAcknowledgementTimeout.Should().Be(
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(8)));
		currentParameters.Retries.Should().Be(Retries.FromValue(Word8.FromValue(7)));
		currentParameters.Level1PasswordVerifier.Verifies(nonVolatilePassword).Should().BeTrue();
		currentParameters.Level1PasswordVerifier.Verifies(permanentPassword).Should().BeFalse();
	}

	private sealed class InMemoryRouterParameterStore :
		IParticipantParameterStore
	{
		private readonly Dictionary<(ParameterTable Table, ParameterNumber Number), ParameterValue> values = [];

		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			this.values.TryGetValue((parameterTable, parameterNumber), out var value);

			return ValueTask.FromResult(value);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default)
		{
			this.values[(parameterTable, parameterNumber)] = parameterValue;

			return ValueTask.CompletedTask;
		}

		public ValueTask<T> ExecuteInitializationAsync<T>(
			Func<CancellationToken, ValueTask<T>> initialize,
			CancellationToken cancellationToken = default)
		{
			return initialize(cancellationToken);
		}
	}

	private sealed class InMemoryRouterPasswordVerifierStore : IRouterPasswordVerifierStore
	{
		private readonly Dictionary<(ParameterTable Table, ParameterNumber Number), PasswordVerifier> values = [];

		public ValueTask<PasswordVerifier?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			this.values.TryGetValue((parameterTable, parameterNumber), out var value);

			return ValueTask.FromResult(value);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			PasswordVerifier passwordVerifier,
			CancellationToken cancellationToken = default)
		{
			this.values[(parameterTable, parameterNumber)] = passwordVerifier;

			return ValueTask.CompletedTask;
		}
	}

	private sealed class MalformedNonVolatilePasswordVerifierStore : IRouterPasswordVerifierStore
	{
		private readonly PasswordVerifier permanentVerifier;

		public MalformedNonVolatilePasswordVerifierStore(PasswordVerifier permanentVerifier)
		{
			this.permanentVerifier = permanentVerifier;
		}

		public ValueTask<PasswordVerifier?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			if (parameterTable == ParameterTable.NonVolatile)
			{
				throw new ArgumentException("Stored verifier version is invalid.");
			}

			return ValueTask.FromResult<PasswordVerifier?>(this.permanentVerifier);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			PasswordVerifier passwordVerifier,
			CancellationToken cancellationToken = default)
		{
			throw new InvalidOperationException("The malformed credential state must not be repaired.");
		}
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private static async Task AssertStoredParameterValueAsync(
		IParticipantParameterStore parameterStore,
		ParameterTable parameterTable,
		byte parameterNumber,
		byte[] expectedWireValue)
	{
		(await parameterStore.GetAsync(
			parameterTable,
			ParameterNumber.FromValue(parameterNumber)))!
			.ToWireValue().Should().Equal(expectedWireValue);
	}

	private static async Task AssertEmptyStoredParameterValueAsync(
		IParticipantParameterStore parameterStore,
		ParameterNumber parameterNumber,
		ParameterValue expectedValue)
	{
		expectedValue.ToWireValue().Should().BeEmpty();
		(await parameterStore.GetAsync(
			ParameterTable.Permanent,
			parameterNumber))!
			.ToWireValue().Should().Equal(expectedValue.ToWireValue());
		(await parameterStore.GetAsync(
			ParameterTable.NonVolatile,
			parameterNumber))!
			.ToWireValue().Should().Equal(expectedValue.ToWireValue());
	}

	private static RouterParameterBootstrapConfiguration CreateBootstrapConfiguration(
		PasswordValue initialLevel1Password)
	{
		return RouterParameterBootstrapConfiguration.FromValues(
			CreateAddress(26, 100, 0),
			CreateNodeName(),
			MaximumMessageLength.FromValue(1_023),
			CreateAddress(26, 100, 25),
			CreateAddress(26, 100, 25),
			initialLevel1Password,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)),
			ManualAcknowledgementTimeout.FromValue(60),
			CreateInstallationTime(),
			MdtTable.FromEntries());
	}

	private static NodeName CreateNodeName()
	{
		return NodeName.FromValue(SevenBitAsciiString.FromValue("Station End"));
	}

	private static TimeAndDate CreateInstallationTime()
	{
		return TimeAndDate.FromValue(SevenBitAsciiString.FromValue("07SEP26154309"));
	}

	private static ParameterValue CreateNeutralCurrentPassword(CommunicationsAddress localAddress)
	{
		return ParameterValue.FromWireValue(
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
				Password.FromValue(PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress).ToWireValue());
	}
}
