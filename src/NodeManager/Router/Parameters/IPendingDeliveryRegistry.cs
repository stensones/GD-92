using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public interface IPendingDeliveryRegistry
{
	RouterParameterRequestStatusIdentifier Reserve(
		CommunicationsAddress source,
		CommunicationsAddress destination);

	RouterParameterRequestStatusIdentifier ReserveNodeLogin(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		CommunicationsAddress userAgentAddress);

	RouterParameterRequestStatusIdentifier ReserveNodeLogoff(
		CommunicationsAddress source,
		CommunicationsAddress destination);

	bool IsPending(RouterParameterRequestStatusIdentifier statusIdentifier);

	RouterParameterRequestStatus? GetStatus(
		RouterParameterRequestStatusIdentifier statusIdentifier);

	bool TryCompleteParameterResponse(Envelope envelope);

	bool TryCompleteAcknowledgement(Envelope envelope);

	bool TryCompleteNegativeAcknowledgement(Envelope envelope);

	bool TryTimeoutParameterRequest(RouterParameterRequestStatusIdentifier statusIdentifier);

	bool TryRecordParameterRequestDeliveryFailure(
		RouterParameterRequestStatusIdentifier statusIdentifier);

	bool TryTimeoutNodeLogin(RouterParameterRequestStatusIdentifier statusIdentifier);
}
