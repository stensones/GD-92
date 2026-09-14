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
	private static readonly ParameterNumber RoutingTableParameterNumber =
		ParameterNumber.FromValue(13);

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

	[HttpPost("{parameterTable}/{parameterNumber}")]
	public async Task<IActionResult> RequestParameter(
		string parameterTable,
		byte parameterNumber,
		CancellationToken cancellationToken)
	{
		if (!ParameterTableRoute.TryParse(parameterTable, out var table))
		{
			return this.BadRequest("Parameter Table must be permanent, non-volatile, or current.");
		}

		var statusIdentifier = await routerParameterRequests.RequestLocalRouterParameter(
			table,
			ParameterNumber.FromValue(parameterNumber),
			cancellationToken);

		return new SeeOtherRedirectResult(
			$"/router/parameters/{parameterTable}/{parameterNumber}/status/{statusIdentifier}");
	}

	[HttpPost("{parameterTable}/{parameterNumber}/entries/{firstEntry}-{lastEntry}")]
	public async Task<IActionResult> RequestParameterEntries(
		string parameterTable,
		byte parameterNumber,
		ushort firstEntry,
		ushort lastEntry,
		CancellationToken cancellationToken)
	{
		if (!ParameterTableRoute.TryParse(parameterTable, out var table))
		{
			return this.BadRequest("Parameter Table must be permanent, non-volatile, or current.");
		}

		var entrySelection = ParameterEntrySelection.Range(
			ParameterEntryIndex.FromValue(firstEntry),
			ParameterEntryIndex.FromValue(lastEntry));
		var statusIdentifier = await routerParameterRequests.RequestLocalRouterParameterEntries(
			table,
			ParameterNumber.FromValue(parameterNumber),
			entrySelection,
			cancellationToken);

		return new SeeOtherRedirectResult(
			$"/router/parameters/{parameterTable}/{parameterNumber}/entries/" +
			$"{firstEntry}-{lastEntry}/status/{statusIdentifier}");
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

	[HttpGet("{parameterTable}/{parameterNumber}/status/{identifier}")]
	public IActionResult ParameterStatus(string parameterTable, byte parameterNumber, string identifier)
	{
		return this.Status(identifier, ParameterNumber.FromValue(parameterNumber));
	}

	[HttpGet("{parameterTable}/{parameterNumber}/entries/{firstEntry}-{lastEntry}/status/{identifier}")]
	public IActionResult ParameterEntryStatus(
		string parameterTable,
		byte parameterNumber,
		ushort firstEntry,
		ushort lastEntry,
		string identifier)
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
		var routingTableEntries = status is ReceivedRouterParameterRequestStatus receivedRoutingTable
			? FormatRoutingTableEntries(parameterNumber, receivedRoutingTable.ParameterValue)
			: null;
		bool? moreValues = status is ReceivedRouterParameterRequestStatus receivedMoreValues
			? receivedMoreValues.MoreValues.Value == ProtocolBoolean.True
			: null;
		string? rejectionReason = status is RejectedRouterParameterRequestStatus rejected
			? FormatRejectionReason(rejected.ReasonCode)
			: null;

		return new RouterParameterRequestStatusResponse(
			status.Identifier.ToString(),
			ManagementTransactionStatusFormatter.GetState(status),
			brigadeOrAgencyNumber,
			userAgentAddress,
			parameterNumber?.Value,
			parameterValue,
			routingTableEntries,
			moreValues,
			rejectionReason);
	}

	private static string FormatRejectionReason(ReasonCode reasonCode)
	{
		ArgumentNullException.ThrowIfNull(reasonCode);

		if (reasonCode.ParameterReasonCode is { } parameterReasonCode)
		{
			return $"Parameter / {FormatReasonCode(parameterReasonCode)}";
		}

		if (reasonCode.GeneralReasonCode is { } generalReasonCode)
		{
			return $"General / {FormatReasonCode(generalReasonCode)}";
		}

		if (reasonCode.PrinterReasonCode is { } printerReasonCode)
		{
			return $"Printer / {FormatReasonCode(printerReasonCode)}";
		}

		throw new InvalidOperationException("The rejection Reason Code has no supported reason set.");
	}

	private static string FormatReasonCode<TReasonCode>(TReasonCode reasonCode)
		where TReasonCode : struct, Enum
	{
		return string.Concat(reasonCode.ToString().Select((character, index) =>
			index > 0 && char.IsUpper(character)
				? $" {character}"
				: character.ToString()));
	}

	private static IReadOnlyList<RoutingTableEntryStatusResponse>? FormatRoutingTableEntries(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		if (parameterNumber != RoutingTableParameterNumber)
		{
			return null;
		}

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var routingTable = RoutingTable.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router Routing Table Parameter contains trailing encoded data.",
				nameof(parameterValue));
		}

		return routingTable.Entries
			.Select(entry => new RoutingTableEntryStatusResponse(
				entry.Index.Value,
				Format(entry.NextNode)))
			.ToArray();
	}

	private static string? FormatParameterValue(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		return parameterNumber?.Value switch
		{
			1 or 12 or 19 => FormatSingleOctet(parameterValue),
			2 => FormatWord16(parameterValue),
			3 => FormatNodeName(parameterValue),
			4 => FormatCurrentPassword(parameterValue),
			5 or 6 or 7 or 8 => "PASSWORD",
			9 => FormatMaximumMessageLength(parameterValue),
			10 => FormatNetworkManagerAddress(parameterValue, 10),
			11 => FormatNetworkManagerAddress(parameterValue, 11),
			18 => FormatManualAcknowledgementTimeout(parameterValue),
			20 => FormatTimeAndDate(parameterValue),
			_ => null
		};
	}

	private static string FormatNetworkManagerAddress(
		ParameterValue parameterValue,
		byte parameterNumber)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var communicationsAddress = CommunicationsAddress.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Router Parameter {parameterNumber} contains trailing encoded data.",
				nameof(parameterValue));
		}

		return Format(communicationsAddress);
	}

	private static string FormatMaximumMessageLength(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var maximumMessageLength = MaximumMessageLength.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router Parameter 9 contains trailing encoded data.",
				nameof(parameterValue));
		}

		return maximumMessageLength.Value.ToString(CultureInfo.InvariantCulture);
	}

	private static string FormatManualAcknowledgementTimeout(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var manualAcknowledgementTimeout =
			ManualAcknowledgementTimeout.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router Parameter 18 contains trailing encoded data.",
				nameof(parameterValue));
		}

		return manualAcknowledgementTimeout.Value.ToString(CultureInfo.InvariantCulture);
	}

	private static string FormatTimeAndDate(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var timeAndDate = TimeAndDate.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router Parameter 20 contains trailing encoded data.",
				nameof(parameterValue));
		}

		return timeAndDate.Value.Value;
	}

	private static string FormatNodeName(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var nodeName = NodeName.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router Parameter 3 contains trailing encoded data.",
				nameof(parameterValue));
		}

		return nodeName.Value.Value;
	}

	private static string FormatWord16(ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = (ushort)buffer.ReadUnsignedBits(16);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router Parameter 2 must contain exactly one encoded word.",
				nameof(parameterValue));
		}

		return value.ToString(CultureInfo.InvariantCulture);
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
