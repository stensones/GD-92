namespace NodeManager.Router.Parameters;

public sealed record RouterParameterRequestStatusResponse(
	string Identifier,
	string State,
	byte? BrigadeOrAgencyNumber,
	string? UserAgentAddress,
	byte? ParameterNumber,
	string? ParameterValue,
	IReadOnlyList<RoutingTableEntryStatusResponse>? RoutingTableEntries,
	bool? MoreValues);

public sealed record RoutingTableEntryStatusResponse(
	ushort Index,
	string NextNode);
