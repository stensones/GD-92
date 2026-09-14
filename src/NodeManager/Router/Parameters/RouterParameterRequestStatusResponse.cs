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
	string? RejectionReason)
{
	public IReadOnlyList<WanTableEntryStatusResponse>? WanTableEntries { get; init; }
	public IReadOnlyList<LanTableEntryStatusResponse>? LanTableEntries { get; init; }
	public IReadOnlyList<IsdnTableEntryStatusResponse>? IsdnTableEntries { get; init; }
}

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

public sealed record WanTableEntryStatusResponse(
	ushort Index,
	bool Used,
	string NextNode,
	string WanAddress,
	string ConnectType);

public sealed record LanTableEntryStatusResponse(
	ushort Index,
	bool Used,
	string NextNode,
	string LanAddress);

public sealed record IsdnTableEntryStatusResponse(
	ushort Index,
	bool Used,
	string NextNode,
	string TelephoneNumber,
	byte HoldTime,
	bool Available);
