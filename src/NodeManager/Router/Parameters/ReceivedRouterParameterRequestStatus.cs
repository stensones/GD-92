using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed record ReceivedRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	ParameterValue ParameterValue) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "received";
}
