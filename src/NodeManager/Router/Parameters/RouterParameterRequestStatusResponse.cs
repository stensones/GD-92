namespace NodeManager.Router.Parameters;

public sealed record RouterParameterRequestStatusResponse(
	string Identifier,
	string State,
	byte? BrigadeOrAgencyNumber,
	string? UserAgentAddress);
