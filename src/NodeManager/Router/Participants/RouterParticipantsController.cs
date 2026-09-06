using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Router.Participants;

[Route("router/participants")]
public sealed class RouterParticipantsController(
	IInventoryScanRunner inventoryScanRunner,
	IInventoryScanRegistry inventoryScans) : Controller
{
	[HttpPost("discovery")]
	public IActionResult StartDiscovery()
	{
		var identifier = inventoryScanRunner.Start();

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

		var status = inventoryScans.Get(statusIdentifier);

		return status is null ? this.NotFound() : this.Ok(status);
	}
}
