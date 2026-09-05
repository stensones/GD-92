using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapper
{
	private readonly IRouterParameterStore store;
	private readonly IRouterLevel1PasswordVerifierStore passwordVerifierStore;

	public RouterParameterBootstrapper(
		IRouterParameterStore store,
		IRouterLevel1PasswordVerifierStore passwordVerifierStore)
	{
		this.store = store ?? throw new ArgumentNullException(nameof(store));
		this.passwordVerifierStore = passwordVerifierStore ??
			throw new ArgumentNullException(nameof(passwordVerifierStore));
	}

	public async ValueTask<RouterCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		RouterParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var brigadeOrAgencyIdentifier = await this.LoadOrBootstrapParameterAsync(
			RouterParameterCatalogue.BrigadeOrAgency,
			configuration.LocalAddress.Brigade.Value,
			cancellationToken);
		var currentPassword = await this.LoadOrBootstrapParameterAsync(
			RouterParameterCatalogue.CurrentPassword,
			CreateNeutralCurrentPassword(configuration.LocalAddress),
			cancellationToken);
		var noAcknowledgementTimeout = await this.LoadOrBootstrapParameterAsync(
			RouterParameterCatalogue.NoAcknowledgementTimeout,
			configuration.NoAcknowledgementTimeout,
			cancellationToken);
		var retries = await this.LoadOrBootstrapParameterAsync(
			RouterParameterCatalogue.Retries,
			configuration.Retries,
			cancellationToken);
		var level1PasswordVerifier = await this.LoadLevel1PasswordVerifierAsync(
			configuration.InitialLevel1Password,
			cancellationToken);

		return RouterCurrentParameterProjection.FromNonVolatileParameters(
			brigadeOrAgencyIdentifier,
			currentPassword,
			level1PasswordVerifier,
			noAcknowledgementTimeout,
			retries);
	}

	private async ValueTask<T> LoadOrBootstrapParameterAsync<T>(
		RouterParameterDefinition<T> parameter,
		T initialValue,
		CancellationToken cancellationToken)
	{
		var permanentValue = await this.store.GetAsync(
			ParameterTable.Permanent,
			parameter.Number,
			cancellationToken);
		if (permanentValue is null)
		{
			permanentValue = parameter.Encode(initialValue);
			await this.store.StoreAsync(
				ParameterTable.Permanent,
				parameter.Number,
				permanentValue,
				cancellationToken);
		}

		var nonVolatileValue = await this.store.GetAsync(
			ParameterTable.NonVolatile,
			parameter.Number,
			cancellationToken);
		if (nonVolatileValue is null)
		{
			nonVolatileValue = permanentValue;
			await this.store.StoreAsync(
				ParameterTable.NonVolatile,
				parameter.Number,
				nonVolatileValue,
				cancellationToken);
		}

		return parameter.Read(nonVolatileValue);
	}

	private async ValueTask<PasswordVerifier> LoadLevel1PasswordVerifierAsync(
		PasswordValue initialLevel1Password,
		CancellationToken cancellationToken)
	{
		var permanentVerifier = await GetPasswordVerifierAsync(
			this.passwordVerifierStore,
			ParameterTable.Permanent,
			cancellationToken);
		var permanentWasMissing = permanentVerifier is null;
		if (permanentWasMissing)
		{
			permanentVerifier = PasswordVerifier.Create(
				initialLevel1Password,
				PasswordVerifierWorkFactor.Default);
			await this.passwordVerifierStore.StoreAsync(
				ParameterTable.Permanent,
				permanentVerifier,
				cancellationToken);
		}

		var nonVolatileVerifier = await GetPasswordVerifierAsync(
			this.passwordVerifierStore,
			ParameterTable.NonVolatile,
			cancellationToken);
		if (nonVolatileVerifier is not null)
		{
			return nonVolatileVerifier;
		}

		if (!permanentWasMissing)
		{
			throw new InvalidOperationException(
				"Router Non-Volatile Parameter 5 password verifier is missing while its Permanent verifier exists.");
		}

		await this.passwordVerifierStore.StoreAsync(
			ParameterTable.NonVolatile,
			permanentVerifier!,
			cancellationToken);

		return permanentVerifier ?? throw new InvalidOperationException(
			"Router Parameter 5 password verifier bootstrap did not produce a verifier.");
	}

	private static async ValueTask<PasswordVerifier?> GetPasswordVerifierAsync(
		IRouterLevel1PasswordVerifierStore passwordVerifierStore,
		ParameterTable parameterTable,
		CancellationToken cancellationToken)
	{
		try
		{
			return await passwordVerifierStore.GetAsync(
				parameterTable,
				cancellationToken);
		}
		catch (ArgumentException exception)
		{
			throw new InvalidOperationException(
				$"Router {ParameterTableName(parameterTable)} Parameter 5 password verifier is malformed.",
				exception);
		}
	}

	private static string ParameterTableName(ParameterTable parameterTable)
	{
		return parameterTable == ParameterTable.Permanent
			? "Permanent"
			: parameterTable == ParameterTable.NonVolatile
				? "Non-Volatile"
				: throw new ArgumentOutOfRangeException(nameof(parameterTable));
	}

	private static PasswordParameter CreateNeutralCurrentPassword(CommunicationsAddress localAddress)
	{
		return PasswordParameter.FromFields(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
			Password.FromValue(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
			localAddress);
	}
}
