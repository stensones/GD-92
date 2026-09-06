using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using NodeManager.Persistence;
using NodeManager.Router.Parameters;

namespace NodeManager.Router.Participants;

public sealed class NodeManagerParticipantIngressReceiver(
	RouterParameterRequestSettings settings,
	NodeManagerCurrentParameterProjectionSource currentParameters,
	IUserAgentIngressReceiver responses,
	IRouterIngress routerIngress) : ILocalParticipantIngressReceiver
{
	private static readonly ParameterNumber PortNumberParameterNumber = ParameterNumber.FromValue(1);
	private static readonly ParameterNumber AgentTypeParameterNumber = ParameterNumber.FromValue(2);

	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != settings.MessageOriginator ||
			envelope.Contents is not ParameterRequest parameterRequest ||
			parameterRequest.ParameterTable != ParameterTable.Current ||
			(parameterRequest.ParameterNumber != PortNumberParameterNumber &&
				parameterRequest.ParameterNumber != AgentTypeParameterNumber))
		{
			await responses.ReceiveAsync(envelope, cancellationToken);
			return;
		}

		var response = Envelope.CreateParameterResponse(
			envelope,
			settings.MessageOriginator,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
			Parameter.FromFields(
				MoreValues.No,
				currentParameters.GetCurrent().Get(parameterRequest.ParameterNumber)));

		await routerIngress.SubmitAsync(response, cancellationToken);
	}
}
