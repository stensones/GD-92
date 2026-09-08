using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public interface IManagementTransactionRegistry :
	IManagementTransactionStatusReader
{
	RouterParameterRequestStatusIdentifier ReserveParameterRequest(
		CommunicationsAddress source,
		CommunicationsAddress destination);

	RouterParameterRequestStatusIdentifier ReserveNodeLogin(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		CommunicationsAddress userAgentAddress);

	RouterParameterRequestStatusIdentifier ReserveNodeLogoff(
		CommunicationsAddress source,
		CommunicationsAddress destination);

	bool IsActive(RouterParameterRequestStatusIdentifier statusIdentifier);

	bool IsAwaitingFinalResponse(RouterParameterRequestStatusIdentifier statusIdentifier);

	bool TryCompleteParameterResponse(Envelope envelope);

	bool TryCompleteAcknowledgement(Envelope envelope);

	bool TryCompleteNegativeAcknowledgement(Envelope envelope);

	bool TryTimeout(RouterParameterRequestStatusIdentifier statusIdentifier);

	bool TryRecordDeliveryFailure(
		RouterParameterRequestStatusIdentifier statusIdentifier);
}
