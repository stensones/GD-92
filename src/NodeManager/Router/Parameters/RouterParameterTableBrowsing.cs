using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record RouterParameterTableDefinition(
	ParameterNumber ParameterNumber,
	string Name);

public sealed record RouterParameterTableSelection(
	string ParameterTable,
	byte ParameterNumber,
	ushort FirstEntry,
	ushort LastEntry,
	IReadOnlyList<RouterParameterTableDefinition> Tables,
	string? ValidationMessage = null);

public sealed record RouterParameterTableStatus(
	RouterParameterTableSelection Selection,
	RouterParameterTableDefinition Table,
	RouterParameterRequestStatusResponse Response)
{
	public ushort? NextFirstEntry =>
		this.Response is
		{
			State: "received",
			MoreValues: true,
			RoutingTableEntries.Count: > 0
		} &&
		this.Response.RoutingTableEntries[^1].Index < this.Selection.LastEntry
			? (ushort)(this.Response.RoutingTableEntries[^1].Index + 1)
			: null;
}

public static class RouterParameterTables
{
	private static readonly IReadOnlyList<RouterParameterTableDefinition> supported =
	[
		new(ParameterNumber.FromValue(13), "Routing Table"),
		new(ParameterNumber.FromValue(14), "PSTN Table"),
		new(ParameterNumber.FromValue(15), "WAN Table"),
		new(ParameterNumber.FromValue(16), "LAN Table"),
		new(ParameterNumber.FromValue(17), "ISDN Table"),
		new(ParameterNumber.FromValue(21), "MDT Table")
	];

	public static IReadOnlyList<RouterParameterTableDefinition> Supported => supported;

	public static bool TryGet(
		ParameterNumber parameterNumber,
		out RouterParameterTableDefinition table)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		var matchedTable = supported.SingleOrDefault(candidate =>
			candidate.ParameterNumber == parameterNumber);
		table = matchedTable!;
		return matchedTable is not null;
	}
}
