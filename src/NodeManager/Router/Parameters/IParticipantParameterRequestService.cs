using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public interface IParticipantParameterRequestService
{
	Task<RouterParameterRequestStatusIdentifier> RequestCurrentParameter(
		CommunicationsAddress destination,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken);
}
