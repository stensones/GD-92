using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Parameters;

[Route("parameters")]
public sealed class ParametersController : Controller
{
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
		[FromQuery] string? parameterTable)
	{
		var isRouter = string.Equals(agentType, "Router", StringComparison.Ordinal);
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

public sealed record ParameterCatalogueEntry(byte Number, string Name);
