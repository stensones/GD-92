using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;

namespace NodeManager.Network;

[Route("network")]
public sealed class NetworkController : Controller
{
	private readonly RouterParameterRequestSettings requestSettings;

	public NetworkController(RouterParameterRequestSettings requestSettings)
	{
		this.requestSettings = requestSettings;
	}

	[HttpGet]
	public IActionResult Index([FromQuery] string? address)
	{
		this.ViewData["SelectedCommunicationsAddress"] = address ??
			FormatAddress(this.requestSettings.LocalRouter);

		return this.View();
	}

	private static string FormatAddress(Stensones.GD92.Fields.CommunicationsAddress address)
	{
		return FormattableString.Invariant(
			$"{address.Brigade.Value}.{address.Node.Value}.{address.Port.Value}");
	}
}
