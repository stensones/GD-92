using Microsoft.AspNetCore.Mvc;

namespace NodeManager.Router.Parameters;

[Route("router/parameters")]
public sealed class RouterParametersController(
	IRouterParameterRequestService routerParameterRequests,
	IPendingDeliveryRegistry pendingDeliveries) : Controller
{
	[HttpPost("brigade-or-agency-number")]
	public async Task<IActionResult> RequestBrigadeOrAgencyNumber(CancellationToken cancellationToken)
	{
		var statusIdentifier = await routerParameterRequests
			.RequestLocalRouterBrigadeOrAgencyNumber(cancellationToken);

		return new SeeOtherRedirectResult($"/router/parameters/status/{statusIdentifier}");
	}

	[HttpGet("status/{identifier}")]
	public IActionResult Status(string identifier)
	{
		if (!RouterParameterRequestStatusIdentifier.TryParse(identifier, out var statusIdentifier) ||
			!pendingDeliveries.IsPending(statusIdentifier))
		{
			return this.NotFound();
		}

		return this.Ok(new PendingRouterParameterRequestStatus(statusIdentifier));
	}
}
