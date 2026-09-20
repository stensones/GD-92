using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Parameters;

[Route("parameters")]
public sealed class ParametersController : Controller
{
	private static readonly IReadOnlyList<ParameterCatalogueEntry>
		RouterParameterCatalogue =
		[
			new(1, "Brigade or Agency", SupportsScalarRequest: true),
			new(2, "Node Number", SupportsScalarRequest: true),
			new(3, "Node Name", SupportsScalarRequest: true),
			new(4, "Current Password", ParameterCatalogueEntryKind.ProtectedScalar),
			new(5, "Level 1 Password", ParameterCatalogueEntryKind.ProtectedScalar),
			new(6, "Level 2 Password", ParameterCatalogueEntryKind.ProtectedScalar),
			new(7, "Level 3 Password", ParameterCatalogueEntryKind.ProtectedScalar),
			new(8, "Level 4 Password", ParameterCatalogueEntryKind.ProtectedScalar),
			new(9, "Maximum Message Length", SupportsScalarRequest: true),
			new(10, "Network Manager Address 1", SupportsScalarRequest: true),
			new(11, "Network Manager Address 2", SupportsScalarRequest: true),
			new(12, "No Acknowledgement Timeout", SupportsScalarRequest: true),
			new(13, "Routing Table", ParameterCatalogueEntryKind.Table),
			new(14, "PSTN Table", ParameterCatalogueEntryKind.Table),
			new(15, "WAN Table", ParameterCatalogueEntryKind.Table),
			new(16, "LAN Table", ParameterCatalogueEntryKind.Table),
			new(17, "ISDN Table", ParameterCatalogueEntryKind.Table),
			new(18, "Manual Acknowledgement Timeout", SupportsScalarRequest: true),
			new(19, "Retries", SupportsScalarRequest: true),
			new(20, "Time and Date", SupportsScalarRequest: true),
			new(21, "MDT Table", ParameterCatalogueEntryKind.Table)
		];

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<ParameterCatalogueEntry>>
		ParticipantCurrentParameterCatalogues =
			new Dictionary<string, IReadOnlyList<ParameterCatalogueEntry>>(StringComparer.Ordinal)
			{
				["LAN MTA (10)"] =
				[
					new(1, "Port Number"),
					new(2, "Agent Type"),
					new(3, "Interface Status"),
					new(4, "Notify Status Changes"),
					new(5, "Frame Transmit Count"),
					new(6, "Frame Receive Count"),
					new(7, "Frame Transmit Failure Count"),
					new(8, "Frame Receive Failure Count"),
					new(9, "Minimum Message Priority"),
					new(10, "Next Nodes"),
					new(21, "LAN Address")
				],
				["Printer (4)"] =
				[
					new(1, "Port Number"),
					new(2, "Agent Type"),
					new(3, "Control Address"),
					new(21, "Default Source"),
					new(22, "Notify Printer Available"),
					new(23, "Alternative Source"),
					new(24, "Reprint Message")
				],
				["Network Management UA (12)"] =
				[
					new(1, "Port Number"),
					new(2, "Agent Type"),
					new(3, "Control Address")
				]
			};

	[HttpGet]
	public IActionResult Index(
		[FromQuery] string? address,
		[FromQuery] string? agentType,
		[FromQuery] string? parameterTable,
		[FromQuery] byte? selectedParameter,
		[FromQuery] byte? selectedTableParameter)
	{
		var isRouter = string.Equals(agentType, "Router", StringComparison.Ordinal);
		var selectedRouterTable = isRouter
			? RouterParameterTables.Supported.SingleOrDefault(
				table => table.ParameterNumber.Value == selectedTableParameter)
				?? RouterParameterTables.Supported.Single(
					table => table.ParameterNumber.Value == 13)
			: null;
		IReadOnlyList<ParameterCatalogueEntry>? participantParameters = null;
		var hasParticipantCatalogue =
			agentType is not null &&
			ParticipantCurrentParameterCatalogues.TryGetValue(
				agentType,
				out participantParameters);

		this.ViewData["SelectedCommunicationsAddress"] = address;
		this.ViewData["AgentType"] = agentType;
		this.ViewData["ParameterTable"] =
			parameterTable is not null &&
			ParameterTableRoute.TryParse(parameterTable, out _)
				? parameterTable
				: "current";
		this.ViewData["IsRouter"] = isRouter;
		this.ViewData["ParticipantParameters"] = participantParameters ?? [];
		this.ViewData["ParticipantPort"] =
			hasParticipantCatalogue && TryGetParticipantPort(address, out var port)
				? port
				: null;
		this.ViewData["ParticipantParametersAvailable"] =
			hasParticipantCatalogue && this.ViewData["ParticipantPort"] is not null;
		this.ViewData["RouterParameters"] = RouterParameterCatalogue;
		this.ViewData["SelectedRouterTable"] = selectedRouterTable;
		this.ViewData["SelectedRouterParameter"] = isRouter
			? RouterParameterCatalogue.SingleOrDefault(
				parameter =>
					parameter.Number == selectedParameter &&
					parameter.SupportsScalarRequest)
			: null;

		return this.View();
	}

	private static bool TryGetParticipantPort(string? address, out byte port)
	{
		port = default;
		if (string.IsNullOrWhiteSpace(address))
		{
			return false;
		}

		var parts = address.Split('.', StringSplitOptions.None);
		return parts.Length == 3 &&
			byte.TryParse(parts[0], out _) &&
			ushort.TryParse(parts[1], out var node) &&
			node <= 1023 &&
			byte.TryParse(parts[2], out port) &&
			port is > 0 and <= 63;
	}
}

public enum ParameterCatalogueEntryKind
{
	ReadableScalar,
	ProtectedScalar,
	Table
}

public sealed record ParameterCatalogueEntry(
	byte Number,
	string Name,
	ParameterCatalogueEntryKind Kind = ParameterCatalogueEntryKind.ReadableScalar,
	bool SupportsScalarRequest = false);
