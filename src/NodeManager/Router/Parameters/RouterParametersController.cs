using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

[Route("router/parameters")]
public sealed class RouterParametersController(
	IRouterParameterRequestService routerParameterRequests,
	ManagementTransactions managementTransactions) : Controller
{
	[HttpPost("brigade-or-agency-number")]
	public async Task<IActionResult> RequestBrigadeOrAgencyNumber(CancellationToken cancellationToken)
	{
		var statusIdentifier = await routerParameterRequests
			.RequestLocalRouterBrigadeOrAgencyNumber(cancellationToken);

		return new SeeOtherRedirectResult($"/router/parameters/status/{statusIdentifier}");
	}

	[HttpPost("current/{parameterNumber}")]
	public async Task<IActionResult> RequestCurrentParameter(
		byte parameterNumber,
		CancellationToken cancellationToken)
	{
		var statusIdentifier = await routerParameterRequests.RequestLocalRouterCurrentParameter(
			ParameterNumber.FromValue(parameterNumber),
			cancellationToken);

		return new SeeOtherRedirectResult(
			$"/router/parameters/current/{parameterNumber}/status/{statusIdentifier}");
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
		return this.Status(identifier, null);
	}

	[HttpGet("current/{parameterNumber}/status/{identifier}")]
	public IActionResult CurrentStatus(byte parameterNumber, string identifier)
	{
		return this.Status(identifier, ParameterNumber.FromValue(parameterNumber));
	}

	private IActionResult Status(
		string identifier,
		ParameterNumber? parameterNumber)
	{
		if (!RouterParameterRequestStatusIdentifier.TryParse(identifier, out var statusIdentifier))
		{
			return this.NotFound();
		}

		var status = managementTransactions.GetStatus(statusIdentifier);

		if (status is null)
		{
			return this.NotFound();
		}

		return this.Ok(ToResponse(status, parameterNumber));
	}

	private static RouterParameterRequestStatusResponse ToResponse(
		RouterParameterRequestStatus status,
		ParameterNumber? parameterNumber)
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
		string? parameterValue = status is ReceivedRouterParameterRequestStatus receivedParameter
			? FormatParameterValue(parameterNumber, receivedParameter.ParameterValue)
			: null;

		return new RouterParameterRequestStatusResponse(
			status.Identifier.ToString(),
			ManagementTransactionStatusFormatter.GetState(status),
			brigadeOrAgencyNumber,
			userAgentAddress,
			parameterNumber?.Value,
			parameterValue);
	}

	private static string? FormatParameterValue(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		return parameterNumber?.Value switch
		{
			1 or 12 or 19 => FormatSingleOctet(parameterValue),
			4 => FormatCurrentPassword(parameterValue),
			5 => "PASSWORD",
			_ => null
		};
	}

	private static string? FormatSingleOctet(ParameterValue parameterValue)
	{
		return parameterValue.ToWireValue() switch
		{
			[var value] => value.ToString(CultureInfo.InvariantCulture),
			_ => null
		};
	}

	private static string FormatCurrentPassword(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var currentPassword = PasswordParameter.FromEncodedMessageBuffer(ref buffer);

		return string.Join(
			", ",
			$"Level {(byte)currentPassword.Level.Value}",
			$"User-Agent {Format(currentPassword.CommunicationsAddress)}",
			"PASSWORD");
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
