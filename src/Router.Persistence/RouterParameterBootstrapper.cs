using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapper
{
	private static readonly ParameterNumber BrigadeOrAgencyNumber =
		ParameterNumber.FromValue(1);
	private static readonly ParameterNumber CurrentPasswordParameterNumber =
		ParameterNumber.FromValue(4);
	private static readonly ParameterNumber Level1PasswordParameterNumber =
		ParameterNumber.FromValue(5);
	private static readonly ParameterNumber NoAcknowledgementTimeoutParameterNumber =
		ParameterNumber.FromValue(12);
	private static readonly ParameterNumber RetriesParameterNumber =
		ParameterNumber.FromValue(19);

	private readonly IRouterParameterStore store;
	private readonly IRouterPasswordVerifierStore passwordVerifierStore;

	public RouterParameterBootstrapper(
		IRouterParameterStore store,
		IRouterPasswordVerifierStore passwordVerifierStore)
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
			BrigadeOrAgencyNumber,
			ParameterValue.FromWireValue(configuration.LocalAddress.Brigade.Value.ToWireValue()),
			cancellationToken);
		var currentPassword = await this.LoadOrBootstrapParameterAsync(
			CurrentPasswordParameterNumber,
			CreateNeutralCurrentPassword(configuration.LocalAddress),
			cancellationToken);
		var noAcknowledgementTimeout = await this.LoadOrBootstrapParameterAsync(
			NoAcknowledgementTimeoutParameterNumber,
			ParameterValue.FromWireValue(configuration.NoAcknowledgementTimeout.ToWireValue()),
			cancellationToken);
		var retries = await this.LoadOrBootstrapParameterAsync(
			RetriesParameterNumber,
			ParameterValue.FromWireValue(configuration.Retries.ToWireValue()),
			cancellationToken);
		var level1PasswordVerifier = await this.LoadLevel1PasswordVerifierAsync(
			configuration.InitialLevel1Password,
			cancellationToken);

		return RouterCurrentParameterProjection.FromNonVolatileValues(
			brigadeOrAgencyIdentifier,
			currentPassword,
			level1PasswordVerifier,
			noAcknowledgementTimeout,
			retries);
	}

	private async ValueTask<ParameterValue> LoadOrBootstrapParameterAsync(
		ParameterNumber parameterNumber,
		ParameterValue initialValue,
		CancellationToken cancellationToken)
	{
		var permanentValue = await this.store.GetAsync(
			ParameterTable.Permanent,
			parameterNumber,
			cancellationToken);
		if (permanentValue is null)
		{
			permanentValue = initialValue;
			await this.store.StoreAsync(
				ParameterTable.Permanent,
				parameterNumber,
				permanentValue,
				cancellationToken);
		}

		var nonVolatileValue = await this.store.GetAsync(
			ParameterTable.NonVolatile,
			parameterNumber,
			cancellationToken);
		if (nonVolatileValue is null)
		{
			nonVolatileValue = permanentValue;
			await this.store.StoreAsync(
				ParameterTable.NonVolatile,
				parameterNumber,
				nonVolatileValue,
				cancellationToken);
		}

		return nonVolatileValue;
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
				Level1PasswordParameterNumber,
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
			Level1PasswordParameterNumber,
			permanentVerifier!,
			cancellationToken);

		return permanentVerifier ?? throw new InvalidOperationException(
			"Router Parameter 5 password verifier bootstrap did not produce a verifier.");
	}

	private static async ValueTask<PasswordVerifier?> GetPasswordVerifierAsync(
		IRouterPasswordVerifierStore passwordVerifierStore,
		ParameterTable parameterTable,
		CancellationToken cancellationToken)
	{
		try
		{
			return await passwordVerifierStore.GetAsync(
				parameterTable,
				Level1PasswordParameterNumber,
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

	private static ParameterValue CreateNeutralCurrentPassword(CommunicationsAddress localAddress)
	{
		return ParameterValue.FromWireValue(
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
				Password.FromValue(
					PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress).ToWireValue());
	}
}
