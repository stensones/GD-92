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
				: null;
	}

	private async ValueTask<Envelope> ModifyRetriesAsync(
		Envelope envelope,
		SetParameter setParameter,
		CancellationToken cancellationToken)
	{
		if (!this.CanModify(setParameter.ParameterTable))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

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

		if (setParameter.ParameterTable != ParameterTable.Current)
		{
			await parameterStore.StoreAsync(
				setParameter.ParameterTable,
				RouterParameterCatalogue.Retries.Number,
				RouterParameterCatalogue.Retries.Encode(retries),
				cancellationToken);
		}

		if (setParameter.ParameterTable != ParameterTable.Permanent &&
			!currentParameterSource.TryChangeRetries(retries))
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
		if (setParameter.ParameterTable != ParameterTable.NonVolatile ||
			!this.CanModify(setParameter.ParameterTable))
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

		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Number,
			RouterParameterCatalogue.NoAcknowledgementTimeout.Encode(noAcknowledgementTimeout),
			cancellationToken);

		if (!currentParameterSource.TryChangeNoAcknowledgementTimeout(noAcknowledgementTimeout))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
	}

	private bool CanModify(ParameterTable table)
	{
		return (table == ParameterTable.Current ||
			table == ParameterTable.NonVolatile ||
			table == ParameterTable.Permanent) &&
			currentParameterSource.HasActiveNodeLoginAtLevelOne();
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
