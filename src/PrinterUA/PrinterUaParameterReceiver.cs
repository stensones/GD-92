using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace PrinterUA;

public sealed class PrinterUaParameterReceiver(
	PrinterUaSettings settings,
	IRouterIngress routerIngress) : ILocalParticipantIngressReceiver
{
	private static readonly ParameterNumber AgentTypeParameterNumber = ParameterNumber.FromValue(2);

	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0] != settings.LocalAddress ||
			envelope.Contents is not ParameterRequest parameterRequest ||
			parameterRequest.ParameterTable != ParameterTable.Current ||
			parameterRequest.ParameterNumber != AgentTypeParameterNumber)
		{
			return;
		}

		var response = Envelope.CreateParameterResponse(
			envelope,
			settings.LocalAddress,
			settings.ProtocolVersion,
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue([4])));

		await routerIngress.SubmitAsync(response, cancellationToken);
	}
}
