using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public interface IParticipantParameterRequestService
{
	Task<RouterParameterRequestStatusIdentifier> RequestParameter(
		CommunicationsAddress destination,
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken);
}
