namespace NodeManager.Router.Parameters;

public interface IRouterParameterRequestService
{
	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken);
}
