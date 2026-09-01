using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class RouterParameterRequestService(
	RouterParameterRequestSettings settings,
	IPendingDeliveryRegistry pendingDeliveries,
	IRouterIngress routerIngress) : IRouterParameterRequestService
{
	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken)
	{
		var statusIdentifier = pendingDeliveries.Reserve(
			settings.MessageOriginator,
			settings.LocalRouter);
		var envelope = Envelope.FromValues(
			settings.MessageOriginator,
			Destinations.FromAddresses(settings.LocalRouter),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				statusIdentifier.USWR.SequenceNumber,
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(1)));

		await routerIngress.SubmitAsync(envelope, cancellationToken);
		return statusIdentifier;
	}
}
