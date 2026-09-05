using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record PendingNodeLoginStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	CommunicationsAddress UserAgentAddress) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "pending";
}

public sealed record PendingNodeLogoffStatus(
	RouterParameterRequestStatusIdentifier Identifier) :
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

public sealed record LoggedOffNodeLoginStatus(
	RouterParameterRequestStatusIdentifier Identifier) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "logged-off";
}

public sealed record InvalidPasswordNodeLoginStatus(
	RouterParameterRequestStatusIdentifier Identifier,
	CommunicationsAddress UserAgentAddress) :
	RouterParameterRequestStatus(Identifier)
{
	public string State { get; } = "invalid_password";
}
