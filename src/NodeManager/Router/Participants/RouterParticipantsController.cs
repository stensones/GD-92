using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Router.Participants;

[Route("router/participants")]
public sealed class RouterParticipantsController(
	InventoryScan inventoryScan) : Controller
{
	[HttpPost("discovery")]
	public IActionResult StartDiscovery()
	{
		var identifier = inventoryScan.Start();

		return new SeeOtherRedirectResult($"/router/participants/discovery/status/{identifier}");
	}

	[HttpGet("discovery/status/{identifier}")]
	public IActionResult Status(string identifier)
	{
		if (!InventoryScanStatusIdentifier.TryParse(identifier, out var statusIdentifier) ||
			statusIdentifier is null)
		{
			return this.NotFound();
		}

		var status = inventoryScan.Get(statusIdentifier);

		return status is null ? this.NotFound() : this.Ok(status);
	}
}
