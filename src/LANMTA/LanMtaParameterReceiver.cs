using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using LANMTA.Persistence;
using Stensones.GD92.Transport.RabbitMQ;

namespace LANMTA;

public sealed class LanMtaParameterReceiver(
	LanMtaSettings settings,
	LanMtaCurrentParameterProjectionSource currentParameters,
	IRouterIngress routerIngress) : ILocalParticipantIngressReceiver
{
	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != settings.LocalAddress ||
			envelope.Contents is not ParameterRequest parameterRequest ||
			parameterRequest.ParameterTable != ParameterTable.Current)
		{
			return;
		}

		var projection = currentParameters.GetCurrent();
		if (!projection.TryGet(parameterRequest.ParameterNumber, out var parameterValue))
		{
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
}
