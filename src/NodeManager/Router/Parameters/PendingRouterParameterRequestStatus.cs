namespace NodeManager.Router.Parameters;

public sealed record PendingRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier)
{
	public string State { get; } = "pending";
}
