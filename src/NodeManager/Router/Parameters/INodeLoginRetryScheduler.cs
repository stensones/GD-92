using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public interface INodeLoginRetryScheduler
{
	void Schedule(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		Envelope envelope);
}
