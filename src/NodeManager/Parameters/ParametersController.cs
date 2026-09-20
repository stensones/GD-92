using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Parameters;

[Route("parameters")]
public sealed class ParametersController : Controller
{
	[HttpGet]
	public IActionResult Index(
		[FromQuery] string? address,
		[FromQuery] string? agentType,
		[FromQuery] string? parameterTable)
	{
		this.ViewData["SelectedCommunicationsAddress"] = address;
		this.ViewData["AgentType"] = agentType;
		this.ViewData["ParameterTable"] =
			parameterTable is not null &&
			ParameterTableRoute.TryParse(parameterTable, out _)
				? parameterTable
				: "current";

		return this.View();
	}
}
