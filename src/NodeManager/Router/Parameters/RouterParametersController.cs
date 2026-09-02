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
		if (!RouterParameterRequestStatusIdentifier.TryParse(identifier, out var statusIdentifier))
		{
			return this.NotFound();
		}

		var status = pendingDeliveries.GetStatus(statusIdentifier);

		if (status is null)
		{
			return this.NotFound();
		}

		return this.Ok(ToResponse(status));
	}

	private static RouterParameterRequestStatusResponse ToResponse(
		RouterParameterRequestStatus status)
	{
		byte? brigadeOrAgencyNumber = status is ReceivedRouterParameterRequestStatus receivedStatus
			? receivedStatus.ParameterValue.ToWireValue() switch
			{
				[var value] => value,
				_ => null
			}
			: null;

		return new RouterParameterRequestStatusResponse(
			status.Identifier.ToString(),
			status switch
			{
				PendingRouterParameterRequestStatus pending => pending.State,
				ReceivedRouterParameterRequestStatus received => received.State,
				_ => throw new InvalidOperationException("The Router Parameter Request status is unknown.")
			},
			brigadeOrAgencyNumber);
	}
}
