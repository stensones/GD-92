namespace NodeManager.Router.Parameters;

public interface IManagementTransactionStatusReader
{
	RouterParameterRequestStatus? GetStatus(
		RouterParameterRequestStatusIdentifier statusIdentifier);
}
