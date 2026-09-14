namespace NodeManager.Router.Parameters;

public sealed record RouterParameterRequestStatusResponse(
	string Identifier,
	string State,
	byte? BrigadeOrAgencyNumber,
	string? UserAgentAddress,
	byte? ParameterNumber,
	string? ParameterValue,
	IReadOnlyList<RoutingTableEntryStatusResponse>? RoutingTableEntries,
	IReadOnlyList<PstnTableEntryStatusResponse>? PstnTableEntries,
	bool? MoreValues,
	string? RejectionReason);

public sealed record RoutingTableEntryStatusResponse(
	ushort Index,
	string NextNode);

public sealed record PstnTableEntryStatusResponse(
	ushort Index,
	bool Used,
	string NextNode,
	string TelephoneNumber,
	byte HoldTime,
	bool Available);
