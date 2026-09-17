using ParticipantParameters;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router;

internal sealed class RouterParameterModification(
	CommunicationsAddress localAddress,
	ProtocolVersion protocolVersion,
	RouterCurrentParameterProjectionSource currentParameterSource,
	IParticipantParameterStore parameterStore)
{
	private static readonly PasswordLevel LevelTwo =
		PasswordLevel.FromValue(PasswordLevelNumber.Level2);
	private static readonly PasswordLevel LevelThree =
		PasswordLevel.FromValue(PasswordLevelNumber.Level3);

	public async ValueTask<Envelope?> HandleAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not SetParameter setParameter)
		{
			return null;
		}

		return setParameter.ParameterNumber == RouterParameterCatalogue.Retries.Number
			? await this.ModifyRetriesAsync(envelope, setParameter, cancellationToken)
			: setParameter.ParameterNumber == RouterParameterCatalogue.NoAcknowledgementTimeout.Number
				? await this.ModifyNoAcknowledgementTimeoutAsync(
					envelope,
					setParameter,
					cancellationToken)
				: setParameter.ParameterNumber ==
				RouterParameterCatalogue.NetworkManagerAddress1.Number
				? await this.ModifyNetworkManagerAddress1Async(
					envelope,
					setParameter,
					cancellationToken)
			: setParameter.ParameterNumber ==
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number
				? await this.ModifyManualAcknowledgementTimeoutAsync(
						envelope,
						setParameter,
						cancellationToken)
				: setParameter.ParameterNumber == RouterParameterCatalogue.BrigadeOrAgency.Number
					? await this.ModifyBrigadeOrAgencyAsync(
						envelope,
						setParameter,
						cancellationToken)
				: setParameter.ParameterNumber == RouterParameterCatalogue.MaximumMessageLength.Number
					? await this.ModifyMaximumMessageLengthAsync(
						envelope,
						setParameter,
						cancellationToken)
				: null;
	}

	private async ValueTask<Envelope> ModifyNetworkManagerAddress1Async(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if (setParameter.ParameterTable != ParameterTable.NonVolatile ||
			!currentParameterSource.HasActiveNodeLoginAtOrAbove(LevelTwo, envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		CommunicationsAddress networkManagerAddress;
		try
		{
			networkManagerAddress = RouterParameterCatalogue.NetworkManagerAddress1.Read(
				setParameter.ParameterValue);
		}
		catch (ArgumentException)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.NetworkManagerAddress1.Number,
			RouterParameterCatalogue.NetworkManagerAddress1.Encode(networkManagerAddress),
			cancellationToken);

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private async ValueTask<Envelope> ModifyMaximumMessageLengthAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if (!this.CanModifyMaximumMessageLength(
			setParameter.ParameterTable,
			envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		MaximumMessageLength maximumMessageLength;
		try
		{
			maximumMessageLength = RouterParameterCatalogue.MaximumMessageLength.Read(
				setParameter.ParameterValue);
		}
		catch (ArgumentException)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		if (setParameter.ParameterTable == ParameterTable.NonVolatile)
		{
			await parameterStore.StoreAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.MaximumMessageLength.Number,
				RouterParameterCatalogue.MaximumMessageLength.Encode(maximumMessageLength),
				cancellationToken);
		}

		if (setParameter.ParameterTable == ParameterTable.Current &&
			!currentParameterSource.TryChangeMaximumMessageLength(
				maximumMessageLength,
				LevelTwo,
				envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private async ValueTask<Envelope> ModifyManualAcknowledgementTimeoutAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if ((setParameter.ParameterTable != ParameterTable.Current &&
			setParameter.ParameterTable != ParameterTable.NonVolatile) ||
			!this.CanModify(envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		ManualAcknowledgementTimeout manualAcknowledgementTimeout;
		try
		{
			manualAcknowledgementTimeout =
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Read(
					setParameter.ParameterValue);
		}
		catch (ArgumentException)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		if (setParameter.ParameterTable == ParameterTable.NonVolatile)
		{
			await parameterStore.StoreAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Number,
				RouterParameterCatalogue.ManualAcknowledgementTimeout.Encode(
					manualAcknowledgementTimeout),
				cancellationToken);
		}

		if (setParameter.ParameterTable == ParameterTable.Current &&
			!currentParameterSource.TryChangeManualAcknowledgementTimeout(
				manualAcknowledgementTimeout,
				LevelThree,
				envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private async ValueTask<Envelope> ModifyBrigadeOrAgencyAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if ((setParameter.ParameterTable != ParameterTable.Current &&
			setParameter.ParameterTable != ParameterTable.NonVolatile) ||
			!this.CanModify(envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		if (setParameter.ParameterTable != ParameterTable.NonVolatile)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		BrigadeOrAgencyIdentifier brigadeOrAgency;
		try
		{
			brigadeOrAgency = RouterParameterCatalogue.BrigadeOrAgency.Read(
				setParameter.ParameterValue);
		}
		catch (ArgumentException)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.BrigadeOrAgency.Number,
			RouterParameterCatalogue.BrigadeOrAgency.Encode(brigadeOrAgency),
			cancellationToken);

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private async ValueTask<Envelope> ModifyRetriesAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		Retries retries;
		try
		{
			retries = RouterParameterCatalogue.Retries.Read(setParameter.ParameterValue);
		}
		catch (ArgumentException)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		if (!this.CanModifyRetries(setParameter.ParameterTable, envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		if (setParameter.ParameterTable != ParameterTable.Current)
		{
			await parameterStore.StoreAsync(
				setParameter.ParameterTable,
				RouterParameterCatalogue.Retries.Number,
				RouterParameterCatalogue.Retries.Encode(retries),
				cancellationToken);
		}

		if (setParameter.ParameterTable == ParameterTable.Current &&
			!currentParameterSource.TryChangeRetries(
				retries,
				LevelTwo,
				envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private async ValueTask<Envelope> ModifyNoAcknowledgementTimeoutAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if ((setParameter.ParameterTable != ParameterTable.Current &&
			setParameter.ParameterTable != ParameterTable.NonVolatile) ||
			!this.CanModify(envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		NoAcknowledgementTimeout noAcknowledgementTimeout;
		try
		{
			noAcknowledgementTimeout = RouterParameterCatalogue.NoAcknowledgementTimeout.Read(
				setParameter.ParameterValue);
		}
		catch (ArgumentException)
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.InvalidSyntax);
		}

		if (setParameter.ParameterTable == ParameterTable.NonVolatile)
		{
			await parameterStore.StoreAsync(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
				RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(noAcknowledgementTimeout),
				cancellationToken);
		}

		if (setParameter.ParameterTable == ParameterTable.Current &&
			!currentParameterSource.TryChangeNoAcknowledgementTimeout(
				noAcknowledgementTimeout,
				LevelThree,
				envelope.Source))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private bool CanModify(CommunicationsAddress sourceAddress)
	{
		return currentParameterSource.HasActiveNodeLoginAtOrAbove(LevelThree, sourceAddress);
	}

	private bool CanModifyRetries(
		ParameterTable table,
		CommunicationsAddress sourceAddress)
	{
		return (table == ParameterTable.Current || table == ParameterTable.NonVolatile) &&
			currentParameterSource.HasActiveNodeLoginAtOrAbove(LevelTwo, sourceAddress);
	}

	private bool CanModifyMaximumMessageLength(
		ParameterTable table,
		CommunicationsAddress sourceAddress)
	{
		return (table == ParameterTable.Current ||
			table == ParameterTable.NonVolatile) &&
			currentParameterSource.HasActiveNodeLoginAtOrAbove(LevelTwo, sourceAddress);
	}

	private Envelope CreateNegativeAcknowledgement(
		Envelope envelope,
		ParameterReasonCode reasonCode)
	{
		return Envelope.CreateNegativeAcknowledgement(
			envelope,
			localAddress,
			protocolVersion,
			envelope.Destinations,
			ReasonCode.FromParameterReasonCode(reasonCode));
	}
}
