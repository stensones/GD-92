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

		if (envelope.Contents is not SetParameter
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} setParameter ||
			number != RouterParameterCatalogue.Retries.Number)
		{
			return null;
		}

		if (table != ParameterTable.Current &&
			table != ParameterTable.NonVolatile &&
			table != ParameterTable.Permanent)
		{
			return null;
		}

		if (!currentParameterSource.HasActiveNodeLoginAtLevelOne())
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

		if (table != ParameterTable.Current)
		{
			await parameterStore.StoreAsync(
				table,
				RouterParameterCatalogue.Retries.Number,
				RouterParameterCatalogue.Retries.Encode(retries),
				cancellationToken);
		}

		if (table != ParameterTable.Permanent &&
			!currentParameterSource.TryChangeRetries(retries))
		{
			return this.CreateNegativeAcknowledgement(
				envelope,
				ParameterReasonCode.NoModificationAccess);
		}

		return Envelope.CreateAcknowledgement(envelope, localAddress, protocolVersion);
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
