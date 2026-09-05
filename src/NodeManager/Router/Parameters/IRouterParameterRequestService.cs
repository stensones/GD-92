using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public interface IRouterParameterRequestService
{
	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
		CommunicationsAddress communicationsAddress,
		PasswordValue password,
		CancellationToken cancellationToken);
}
