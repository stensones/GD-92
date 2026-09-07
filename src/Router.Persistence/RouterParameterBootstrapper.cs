using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapper
{
	private readonly ParticipantParameterBootstrapper parameterBootstrapper;
	private readonly IRouterLevel1PasswordVerifierStore passwordVerifierStore;

	public RouterParameterBootstrapper(
		IParticipantParameterStore store,
		IRouterLevel1PasswordVerifierStore passwordVerifierStore)
	{
		this.parameterBootstrapper = new ParticipantParameterBootstrapper(store);
		this.passwordVerifierStore = passwordVerifierStore ??
			throw new ArgumentNullException(nameof(passwordVerifierStore));
	}

	public async ValueTask<RouterCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		RouterParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		return await this.parameterBootstrapper.InitializeAsync(
		[
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.BrigadeOrAgency.Number,
				RouterParameterCatalogue.BrigadeOrAgency.Encode(
					configuration.LocalAddress.Brigade.Value)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.CurrentPassword.Number,
				RouterParameterCatalogue.CurrentPassword.Encode(
					CreateNeutralCurrentPassword(configuration.LocalAddress))),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
					configuration.NoAcknowledgementTimeout)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.Retries.Number,
				RouterParameterCatalogue.Retries.Encode(configuration.Retries))
		],
		(nonVolatileValues, initializeCancellationToken) =>
			this.CreateCurrentParameterProjectionAsync(
				nonVolatileValues,
				configuration.InitialLevel1Password,
				initializeCancellationToken),
		cancellationToken);
	}

	private async ValueTask<RouterCurrentParameterProjection> CreateCurrentParameterProjectionAsync(
		IReadOnlyDictionary<ParameterNumber, ParameterValue> nonVolatileValues,
		PasswordValue initialLevel1Password,
		CancellationToken cancellationToken)
	{
		var level1PasswordVerifier = await this.LoadLevel1PasswordVerifierAsync(
			initialLevel1Password,
			cancellationToken);

		return RouterCurrentParameterProjection.FromNonVolatileParameters(
			RouterParameterCatalogue.BrigadeOrAgency.Read(
				nonVolatileValues[RouterParameterCatalogue.BrigadeOrAgency.Number]),
			RouterParameterCatalogue.CurrentPassword.Read(
				nonVolatileValues[RouterParameterCatalogue.CurrentPassword.Number]),
			level1PasswordVerifier,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Read(
				nonVolatileValues[RouterParameterCatalogue.NoAcknowledgementTimeout.Number]),
			RouterParameterCatalogue.Retries.Read(
				nonVolatileValues[RouterParameterCatalogue.Retries.Number]));
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
