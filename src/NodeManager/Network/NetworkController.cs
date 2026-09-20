using Microsoft.AspNetCore.Mvc;

namespace NodeManager.Network;

[Route("network")]
public sealed class NetworkController : Controller
{
	[HttpGet]
	public IActionResult Index([FromQuery] string? address)
	{
		this.ViewData["SelectedCommunicationsAddress"] = address;

		return this.View();
	}
}
