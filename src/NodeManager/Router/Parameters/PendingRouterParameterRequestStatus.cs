namespace NodeManager.Router.Parameters;

public abstract record RouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier);

public sealed record PendingRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "pending";
}
