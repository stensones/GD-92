using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapper
{
	private readonly ParticipantParameterBootstrapper parameterBootstrapper;
	private readonly IRouterPasswordVerifierStore passwordVerifierStore;

	public RouterParameterBootstrapper(
		IParticipantParameterStore store,
		IRouterPasswordVerifierStore passwordVerifierStore)
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
				RouterParameterCatalogue.NodeNumber.Number,
				RouterParameterCatalogue.NodeNumber.Encode(
					configuration.LocalAddress.Node.Value)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.NodeName.Number,
				RouterParameterCatalogue.NodeName.Encode(configuration.NodeName)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.MaximumMessageLength.Number,
				RouterParameterCatalogue.MaximumMessageLength.Encode(
					configuration.MaximumMessageLength)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.NetworkManagerAddress1.Number,
				RouterParameterCatalogue.NetworkManagerAddress1.Encode(
					configuration.NetworkManagerAddress1)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.NetworkManagerAddress2.Number,
				RouterParameterCatalogue.NetworkManagerAddress2.Encode(
					configuration.NetworkManagerAddress2)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.CurrentPassword.Number,
				RouterParameterCatalogue.CurrentPassword.Encode(
					CreateNeutralCurrentPassword(configuration.LocalAddress))),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(
					configuration.NoAcknowledgementTimeout)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.RouterTable.Number,
				RouterParameterCatalogue.RouterTable.Encode(RoutingTable.FromEntries())),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.PstnTable.Number,
				RouterParameterCatalogue.PstnTable.Encode(PstnTable.FromEntries())),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.WanTable.Number,
				RouterParameterCatalogue.WanTable.Encode(WanTable.FromEntries())),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.LanTable.Number,
				RouterParameterCatalogue.LanTable.Encode(LanTable.FromEntries())),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.IsdnTable.Number,
				RouterParameterCatalogue.IsdnTable.Encode(IsdnTable.FromEntries())),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
					configuration.ManualAcknowledgementTimeout)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.Retries.Number,
				RouterParameterCatalogue.Retries.Encode(configuration.Retries)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.TimeAndDate.Number,
				RouterParameterCatalogue.TimeAndDate.Encode(configuration.TimeAndDate)),
			ParameterBootstrapValue.FromValues(
				RouterParameterCatalogue.MdtTable.Number,
				RouterParameterCatalogue.MdtTable.Encode(configuration.MdtTable))
		],
		(nonVolatileValues, initializeCancellationToken) =>
			this.CreateCurrentParameterProjectionAsync(
				nonVolatileValues,
				configuration,
				initializeCancellationToken),
		cancellationToken);
	}

	private async ValueTask<RouterCurrentParameterProjection> CreateCurrentParameterProjectionAsync(
		IReadOnlyDictionary<ParameterNumber, ParameterValue> nonVolatileValues,
		RouterParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken)
	{
		var level1PasswordVerifier = await this.LoadPasswordVerifierAsync(
			RouterParameterCatalogue.Level1PasswordNumber,
			configuration.InitialLevel1Password,
			cancellationToken);
		var level2PasswordVerifier = await this.LoadPasswordVerifierAsync(
			RouterParameterCatalogue.Level2PasswordNumber,
			configuration.InitialLevel2Password,
			cancellationToken);
		var level3PasswordVerifier = await this.LoadPasswordVerifierAsync(
			RouterParameterCatalogue.Level3PasswordNumber,
			configuration.InitialLevel3Password,
			cancellationToken);
		var level4PasswordVerifier = await this.LoadPasswordVerifierAsync(
			RouterParameterCatalogue.Level4PasswordNumber,
			configuration.InitialLevel4Password,
			cancellationToken);

		return RouterCurrentParameterProjection.FromNonVolatileParameters(
			RouterParameterCatalogue.BrigadeOrAgency.Read(
				nonVolatileValues[RouterParameterCatalogue.BrigadeOrAgency.Number]),
			RouterParameterCatalogue.MaximumMessageLength.Read(
				nonVolatileValues[RouterParameterCatalogue.MaximumMessageLength.Number]),
			RouterParameterCatalogue.CurrentPassword.Read(
				nonVolatileValues[RouterParameterCatalogue.CurrentPassword.Number]),
			level1PasswordVerifier,
			level2PasswordVerifier,
			level3PasswordVerifier,
			level4PasswordVerifier,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Read(
				nonVolatileValues[RouterParameterCatalogue.NoAcknowledgementTimeout.Number]),
			RouterParameterCatalogue.ManualAcknowledgementTimeout.Read(
				nonVolatileValues[
					RouterParameterCatalogue.ManualAcknowledgementTimeout.Number]),
			RouterParameterCatalogue.Retries.Read(
				nonVolatileValues[RouterParameterCatalogue.Retries.Number]));
	}

	private async ValueTask<PasswordVerifier> LoadPasswordVerifierAsync(
		ParameterNumber parameterNumber,
		PasswordValue initialPassword,
		CancellationToken cancellationToken)
	{
		var permanentVerifier = await GetPasswordVerifierAsync(
			this.passwordVerifierStore,
			ParameterTable.Permanent,
			parameterNumber,
			cancellationToken);
		var permanentWasMissing = permanentVerifier is null;
		if (permanentWasMissing)
		{
			permanentVerifier = PasswordVerifier.Create(
				initialPassword,
				PasswordVerifierWorkFactor.Default);
			await this.passwordVerifierStore.StoreAsync(
				ParameterTable.Permanent,
				parameterNumber,
				permanentVerifier,
				cancellationToken);
		}

		var nonVolatileVerifier = await GetPasswordVerifierAsync(
			this.passwordVerifierStore,
			ParameterTable.NonVolatile,
			parameterNumber,
			cancellationToken);
		if (nonVolatileVerifier is not null)
		{
			return nonVolatileVerifier;
		}

		if (!permanentWasMissing)
		{
			throw new InvalidOperationException(
				$"Router Non-Volatile Parameter {parameterNumber.Value} password verifier is missing while its Permanent verifier exists.");
		}

		await this.passwordVerifierStore.StoreAsync(
			ParameterTable.NonVolatile,
			parameterNumber,
			permanentVerifier!,
			cancellationToken);

		return permanentVerifier ?? throw new InvalidOperationException(
			$"Router Parameter {parameterNumber.Value} password verifier bootstrap did not produce a verifier.");
	}

	private static async ValueTask<PasswordVerifier?> GetPasswordVerifierAsync(
		IRouterPasswordVerifierStore passwordVerifierStore,
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken)
	{
		try
		{
			return await passwordVerifierStore.GetAsync(
				parameterTable,
				parameterNumber,
				cancellationToken);
		}
		catch (ArgumentException exception)
		{
			throw new InvalidOperationException(
				$"Router {ParameterTableName(parameterTable)} Parameter {parameterNumber.Value} password verifier is malformed.",
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
