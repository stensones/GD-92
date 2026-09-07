using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Stensones.GD92.Fields;

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

	[HttpPost("logon")]
	[RequireHttps]
	public async Task<IActionResult> LogOn(
		[FromForm(Name = "password")] string password,
		[FromForm(Name = "brigade")] byte brigade,
		[FromForm(Name = "node")] ushort node,
		[FromForm(Name = "port")] byte port,
		CancellationToken cancellationToken)
	{
		var communicationsAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
		var statusIdentifier = await routerParameterRequests.RequestLocalRouterLogon(
			communicationsAddress,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue(password)),
			cancellationToken);

		return new SeeOtherRedirectResult($"/router/parameters/logon/status/{statusIdentifier}");
	}

	[HttpPost("logoff")]
	[RequireHttps]
	public async Task<IActionResult> LogOff(CancellationToken cancellationToken)
	{
		var statusIdentifier = await routerParameterRequests
			.RequestLocalRouterLogoff(cancellationToken);

		return new SeeOtherRedirectResult($"/router/parameters/logoff/status/{statusIdentifier}");
	}

	[HttpGet("status/{identifier}")]
	[HttpGet("logon/status/{identifier}")]
	[HttpGet("logoff/status/{identifier}")]
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
		string? userAgentAddress = status switch
		{
			PendingNodeLoginStatus pendingNodeLogin =>
				Format(pendingNodeLogin.UserAgentAddress),
			LoggedOnNodeLoginStatus loggedOnNodeLogin =>
				Format(loggedOnNodeLogin.UserAgentAddress),
			InvalidPasswordNodeLoginStatus invalidPassword =>
				Format(invalidPassword.UserAgentAddress),
			RejectedNodeLoginStatus rejectedNodeLogin =>
				Format(rejectedNodeLogin.UserAgentAddress),
			TimedOutNodeLoginStatus timedOutNodeLogin =>
				Format(timedOutNodeLogin.UserAgentAddress),
			_ => null
		};

		return new RouterParameterRequestStatusResponse(
			status.Identifier.ToString(),
			status switch
			{
				PendingRouterParameterRequestStatus pending => pending.State,
				ReceivedRouterParameterRequestStatus received => received.State,
				TimedOutRouterParameterRequestStatus timedOut => timedOut.State,
				PendingNodeLoginStatus pending => pending.State,
				PendingNodeLogoffStatus pending => pending.State,
				LoggedOnNodeLoginStatus loggedOn => loggedOn.State,
				LoggedOffNodeLoginStatus loggedOff => loggedOff.State,
				InvalidPasswordNodeLoginStatus invalidPassword => invalidPassword.State,
				RejectedNodeLoginStatus rejectedNodeLogin => rejectedNodeLogin.State,
				TimedOutNodeLoginStatus timedOutNodeLogin => timedOutNodeLogin.State,
				_ => throw new InvalidOperationException("The Router Parameter Request status is unknown.")
			},
			brigadeOrAgencyNumber,
			userAgentAddress);
	}

	private static string Format(CommunicationsAddress communicationsAddress)
	{
		return string.Join(
			'.',
			communicationsAddress.Brigade.Value.ToString(),
			communicationsAddress.Node.Value.ToString(CultureInfo.InvariantCulture),
			communicationsAddress.Port.Value.ToString(CultureInfo.InvariantCulture));
	}
}
