using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace NodeManager.Router.Parameters;

public sealed class SeeOtherRedirectResult(string location) : ActionResult, IStatusCodeActionResult
{
	public string Location { get; } = location;
	public int? StatusCode => StatusCodes.Status303SeeOther;

	public override void ExecuteResult(ActionContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		context.HttpContext.Response.StatusCode = StatusCodes.Status303SeeOther;
		context.HttpContext.Response.Headers.Location = this.Location;
	}
}
