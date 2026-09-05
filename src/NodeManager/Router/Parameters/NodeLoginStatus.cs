using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record PendingNodeLoginStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	CommunicationsAddress UserAgentAddress) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "pending";
}

public sealed record LoggedOnNodeLoginStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	CommunicationsAddress UserAgentAddress) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "logged-on";
}
