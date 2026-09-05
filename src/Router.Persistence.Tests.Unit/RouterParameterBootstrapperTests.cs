using AwesomeAssertions;
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
	public async Task Seeds_the_initial_typed_Router_Parameters_and_projects_their_non_volatile_values()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var passwordVerifierStore = new InMemoryRouterPasswordVerifierStore();
		var localAddress = CreateAddress(26, 100, 0);
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var configuration = RouterParameterBootstrapConfiguration.FromValues(
			localAddress,
			initialPassword,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)));
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			passwordVerifierStore);

		var currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(configuration);

		currentParameters.BrigadeOrAgencyIdentifier.ToWireValue().Should().Equal([26]);
		currentParameters.CurrentPassword.ToWireValue().Should().Equal(
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.FromValue(0)),
				Password.FromValue(PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress).ToWireValue());
		currentParameters.NoAcknowledgementTimeout.Should().Be(
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)));
		currentParameters.Retries.Should().Be(Retries.FromValue(Word8.FromValue(3)));
		(await parameterStore.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(4)))!
			.ToWireValue().Should().Equal(currentParameters.CurrentPassword.ToWireValue());
		(await parameterStore.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(4)))!
			.ToWireValue().Should().Equal(currentParameters.CurrentPassword.ToWireValue());
		(await parameterStore.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(12)))!
			.ToWireValue().Should().Equal([5]);
		(await parameterStore.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(12)))!
			.ToWireValue().Should().Equal([5]);
		(await parameterStore.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(19)))!
			.ToWireValue().Should().Equal([3]);
		(await parameterStore.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(19)))!
			.ToWireValue().Should().Equal([3]);
		(await parameterStore.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(5)))
			.Should().BeNull();
		(await parameterStore.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(5)))
			.Should().BeNull();
		(await passwordVerifierStore.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(5)))!
			.Verifies(initialPassword).Should().BeTrue();
		(await passwordVerifierStore.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(5)))!
			.Verifies(initialPassword).Should().BeTrue();
		currentParameters.Level1PasswordVerifier.Verifies(initialPassword).Should().BeTrue();
	}

	[Fact]
	public async Task Fails_startup_when_the_non_volatile_Level1_password_verifier_is_missing()
	{
		var parameterStore = new InMemoryRouterParameterStore();
		var passwordVerifierStore = new InMemoryRouterPasswordVerifierStore();
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		await passwordVerifierStore.StoreAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(5),
			PasswordVerifier.Create(initialPassword, PasswordVerifierWorkFactor.Default));
		var bootstrapper = new RouterParameterBootstrapper(
			parameterStore,
			passwordVerifierStore);

		var load = async () => await bootstrapper.LoadCurrentParameterProjectionAsync(
			CreateBootstrapConfiguration(initialPassword));

		await load.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Non-Volatile Parameter 5 password verifier is missing*");
		(await passwordVerifierStore.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(5)))
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
			ParameterNumber.FromValue(5),
			PasswordVerifier.Create(permanentPassword, PasswordVerifierWorkFactor.Default));
		await passwordVerifierStore.StoreAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(5),
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

	private sealed class InMemoryRouterParameterStore : IRouterParameterStore
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

	private static RouterParameterBootstrapConfiguration CreateBootstrapConfiguration(
		PasswordValue initialLevel1Password)
	{
		return RouterParameterBootstrapConfiguration.FromValues(
			CreateAddress(26, 100, 0),
			initialLevel1Password,
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)));
	}

	private static ParameterValue CreateNeutralCurrentPassword(CommunicationsAddress localAddress)
	{
		return ParameterValue.FromWireValue(
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.FromValue(0)),
				Password.FromValue(PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress).ToWireValue());
	}
}
