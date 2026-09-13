using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed record ReceivedRouterParameterRequestStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	MoreValues MoreValues,
	ParameterValue ParameterValue) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "received";
}
