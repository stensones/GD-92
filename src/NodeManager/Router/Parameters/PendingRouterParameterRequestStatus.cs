using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public abstract record RouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier);

public sealed record PendingRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "pending";
}

public sealed record TimedOutRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "timed-out";
}

public sealed record DeliveryFailedRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "delivery-failed";
}

public sealed record RejectedRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	ReasonCode ReasonCode) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "rejected";
}
