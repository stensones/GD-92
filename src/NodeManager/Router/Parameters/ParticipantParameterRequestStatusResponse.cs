namespace NodeManager.Router.Parameters;

public sealed record ParticipantParameterRequestStatusResponse(
	string Identifier,
	string State,
	byte ParameterNumber,
	string? ParameterValue);
