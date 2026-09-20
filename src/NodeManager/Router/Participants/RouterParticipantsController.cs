using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Router.Participants;

[Route("router/participants")]
public sealed class RouterParticipantsController(
	InventoryScan inventoryScan) : Controller
{
	[HttpPost("discovery")]
	public async Task<IActionResult> StartDiscovery()
	{
		if (!ManagementTransactionUiRecipient.TryCreate(
			this.HttpContext,
			out var recipient,
			out var validationError))
		{
			return this.BadRequest(validationError);
		}

		var identifier = inventoryScan.Start();
		if (recipient is not null)
		{
			await inventoryScan.RegisterUiRecipientAsync(
				identifier,
				recipient,
				this.HttpContext.RequestAborted);
		}

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
