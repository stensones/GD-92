using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public interface IPendingDeliveryRegistry
{
	RouterParameterRequestStatusIdentifier Reserve(
		CommunicationsAddress source,
		CommunicationsAddress destination);

	bool IsPending(RouterParameterRequestStatusIdentifier statusIdentifier);
}
