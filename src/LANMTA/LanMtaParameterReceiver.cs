using LANMTA.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace LANMTA;

public sealed class LanMtaParameterReceiver(
	LanMtaSettings settings,
	LanMtaCurrentParameterProjectionSource currentParameters,
	ILanMtaRetainedParameterReader retainedParameters,
	IRouterIngress routerIngress) : ILocalParticipantIngressReceiver
{
	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != settings.LocalAddress ||
			envelope.Contents is not ParameterRequest parameterRequest)
		{
			return;
		}

		var parameterValue = await this.GetParameterValueAsync(parameterRequest, cancellationToken);
		if (parameterValue is null)
		{
			var reasonCode = parameterRequest.ParameterTable != ParameterTable.Current &&
				parameterRequest.ParameterTable != ParameterTable.NonVolatile &&
				parameterRequest.ParameterTable != ParameterTable.Permanent
				? ParameterReasonCode.InvalidTable
				: ParameterReasonCode.InvalidParameter;
			var rejection = Envelope.CreateNegativeAcknowledgement(
				envelope,
				settings.LocalAddress,
				settings.ProtocolVersion,
				envelope.Destinations,
				ReasonCode.FromParameterReasonCode(reasonCode));
			await routerIngress.SubmitAsync(rejection, cancellationToken);
			return;
		}

		var response = Envelope.CreateParameterResponse(
			envelope,
			settings.LocalAddress,
			settings.ProtocolVersion,
			Parameter.FromFields(
				MoreValues.No,
				parameterValue));

		await routerIngress.SubmitAsync(response, cancellationToken);
	}

	private async ValueTask<ParameterValue?> GetParameterValueAsync(
		ParameterRequest parameterRequest,
		CancellationToken cancellationToken)
	{
		if (parameterRequest.ParameterTable == ParameterTable.Current)
		{
			var projection = currentParameters.GetCurrent();
			return projection.TryGet(parameterRequest.ParameterNumber, out var currentValue)
				? currentValue
				: null;
		}

		return parameterRequest.ParameterTable == ParameterTable.NonVolatile ||
			parameterRequest.ParameterTable == ParameterTable.Permanent
			? await retainedParameters.GetAsync(
				parameterRequest.ParameterTable,
				parameterRequest.ParameterNumber,
				cancellationToken)
			: null;
	}
}
