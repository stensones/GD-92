using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using NodeManager.RealTime;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

[Route("router/parameters")]
public sealed class RouterParametersController(
	IRouterParameterRequestService routerParameterRequests,
	ManagementTransactions managementTransactions,
	IRouterSessionAuthorization? sessionAuthorization = null) : Controller
{
	private readonly IRouterSessionAuthorization routerSessionAuthorization =
		sessionAuthorization ?? new RouterSessionAuthorization();
	private static readonly ParameterNumber RoutingTableParameterNumber =
		ParameterNumber.FromValue(13);
	private static readonly ParameterNumber PstnTableParameterNumber =
		ParameterNumber.FromValue(14);
	private static readonly ParameterNumber WanTableParameterNumber =
		ParameterNumber.FromValue(15);
	private static readonly ParameterNumber LanTableParameterNumber =
		ParameterNumber.FromValue(16);
	private static readonly ParameterNumber IsdnTableParameterNumber =
		ParameterNumber.FromValue(17);
	private static readonly ParameterNumber MdtTableParameterNumber =
		ParameterNumber.FromValue(21);

	[HttpPost("brigade-or-agency-number")]
	public Task<IActionResult> RequestBrigadeOrAgencyNumber(CancellationToken cancellationToken) =>
		this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterBrigadeOrAgencyNumber(cancellationToken),
			statusIdentifier => $"/router/parameters/status/{statusIdentifier}",
			cancellationToken);

	[HttpPost("current/{parameterNumber}")]
	public Task<IActionResult> RequestCurrentParameter(
		byte parameterNumber,
		CancellationToken cancellationToken)
	{
		return this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterCurrentParameter(
				ParameterNumber.FromValue(parameterNumber),
				cancellationToken),
			statusIdentifier =>
				$"/router/parameters/current/{parameterNumber}/status/{statusIdentifier}",
			cancellationToken);
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

		return await this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterParameter(
				table,
				ParameterNumber.FromValue(parameterNumber),
				cancellationToken),
			statusIdentifier =>
				$"/router/parameters/{parameterTable}/{parameterNumber}/status/{statusIdentifier}",
			cancellationToken);
	}

	[HttpPost("{parameterTable}/{parameterNumber}/value")]
	[RequireHttps]
	public async Task<IActionResult> ModifyParameter(
		string parameterTable,
		byte parameterNumber,
		[FromForm] string[] value,
		CancellationToken cancellationToken)
	{
		if (this.ControllerContext.HttpContext is not { } httpContext ||
			!BrowserSessionIdentifier.TryGet(httpContext, out var browserSessionIdentifier) ||
			!this.routerSessionAuthorization.IsAuthorized(browserSessionIdentifier))
		{
			return this.StatusCode(StatusCodes.Status403Forbidden);
		}

		if (!ParameterTableRoute.TryParse(parameterTable, out var table))
		{
			return this.BadRequest("Parameter Table must be permanent, non-volatile, or current.");
		}

		if (parameterNumber == 12 &&
			(value is not [var timeoutValue] ||
			 !byte.TryParse(
				 timeoutValue,
				 NumberStyles.None,
				 CultureInfo.InvariantCulture,
				 out var timeout) ||
			 timeout == 0 ||
			 !string.Equals(
				 timeoutValue,
				 timeout.ToString(CultureInfo.InvariantCulture),
				 StringComparison.Ordinal)))
		{
			return this.BadRequest(
				"No Acknowledgement Timeout must be exactly one canonical byte from 1 through 255.");
		}

		var wireValue = new byte[value.Length];
		for (var index = 0; index < value.Length; index++)
		{
			if (!byte.TryParse(value[index], out wireValue[index]))
			{
				return this.BadRequest("Parameter value must contain byte values.");
			}
		}

		return await this.SubmitAndRedirectAsync(
			() => routerParameterRequests.ModifyLocalRouterParameter(
				table,
				ParameterNumber.FromValue(parameterNumber),
				ParameterValue.FromWireValue(wireValue),
				cancellationToken),
			statusIdentifier => $"/router/parameters/status/{statusIdentifier}",
			cancellationToken);
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
		return await this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterParameterEntries(
				table,
				ParameterNumber.FromValue(parameterNumber),
				entrySelection,
				cancellationToken),
			statusIdentifier =>
				$"/router/parameters/{parameterTable}/{parameterNumber}/entries/" +
				$"{firstEntry}-{lastEntry}/status/{statusIdentifier}",
			cancellationToken);
	}

	[HttpGet("tables")]
	public IActionResult TableSelection()
	{
		return this.View(
			"TableSelection",
			CreateTableSelection("current", ParameterNumber.FromValue(13), 1, 1));
	}

	[HttpPost("tables")]
	public async Task<IActionResult> RequestTableEntries(
		[FromForm] string parameterTable,
		[FromForm] byte parameterNumber,
		[FromForm] ushort firstEntry,
		[FromForm] ushort lastEntry,
		CancellationToken cancellationToken)
	{
		if (!TryCreateTableSelection(
			parameterTable,
			ParameterNumber.FromValue(parameterNumber),
			firstEntry,
			lastEntry,
			out var selection,
			out var table,
			out var parameterTableValue))
		{
			return this.View("TableSelection", selection);
		}

		return await this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterParameterEntries(
				parameterTableValue,
				table.ParameterNumber,
				ParameterEntrySelection.Range(
					ParameterEntryIndex.FromValue(firstEntry),
					ParameterEntryIndex.FromValue(lastEntry)),
				cancellationToken),
			statusIdentifier =>
				$"/router/parameters/tables/{parameterTable}/{parameterNumber}/entries/" +
				$"{firstEntry}-{lastEntry}/status/{statusIdentifier}",
			cancellationToken);
	}

	[HttpPost("logon")]
	[RequireHttps]
	public async Task<IActionResult> LogOn(
		[FromForm(Name = "password")] string password,
		[FromForm(Name = "brigade")] byte brigade,
		[FromForm(Name = "node")] ushort node,
		[FromForm(Name = "port")] byte port,
		[FromForm(Name = "passwordLevel")] byte passwordLevel,
		CancellationToken cancellationToken)
	{
		if (!Enum.IsDefined((PasswordLevelNumber)passwordLevel) ||
			passwordLevel == (byte)PasswordLevelNumber.Unauthenticated)
		{
			return this.BadRequest("Password level must be between 1 and 4.");
		}

		var communicationsAddress = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
		return await this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterLogon(
				communicationsAddress,
				PasswordLevel.FromValue((PasswordLevelNumber)passwordLevel),
				PasswordValue.FromValue(SevenBitAsciiString.FromValue(password)),
				cancellationToken),
			statusIdentifier => $"/router/parameters/logon/status/{statusIdentifier}",
			cancellationToken,
			this.routerSessionAuthorization.TrackLogOn);
	}

	[HttpPost("logoff")]
	[RequireHttps]
	public Task<IActionResult> LogOff(CancellationToken cancellationToken) =>
		this.SubmitAndRedirectAsync(
			() => routerParameterRequests.RequestLocalRouterLogoff(cancellationToken),
			statusIdentifier => $"/router/parameters/logoff/status/{statusIdentifier}",
			cancellationToken,
			this.routerSessionAuthorization.TrackLogOff);

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

	[HttpGet("tables/{parameterTable}/{parameterNumber}/entries/{firstEntry}-{lastEntry}/status/{identifier}")]
	public IActionResult TableEntryStatus(
		string parameterTable,
		byte parameterNumber,
		ushort firstEntry,
		ushort lastEntry,
		string identifier)
	{
		if (!TryCreateTableSelection(
			parameterTable,
			ParameterNumber.FromValue(parameterNumber),
			firstEntry,
			lastEntry,
			out var selection,
			out var table,
			out _))
		{
			return this.NotFound();
		}

		if (!RouterParameterRequestStatusIdentifier.TryParse(identifier, out var statusIdentifier))
		{
			return this.NotFound();
		}

		var status = managementTransactions.GetStatus(statusIdentifier);
		if (status is null)
		{
			return this.NotFound();
		}

		return this.View(
			"TableStatus",
			new RouterParameterTableStatus(selection, table, ToResponse(status, table.ParameterNumber)));
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

		if (this.ControllerContext.HttpContext is { } httpContext &&
			BrowserSessionIdentifier.TryGet(httpContext, out var browserSessionIdentifier))
		{
			this.routerSessionAuthorization.Observe(
				browserSessionIdentifier,
				statusIdentifier,
				status);
		}

		return this.Ok(ToResponse(status, parameterNumber));
	}

	private async Task<IActionResult> SubmitAndRedirectAsync(
		Func<Task<RouterParameterRequestStatusIdentifier>> submit,
		Func<RouterParameterRequestStatusIdentifier, string> statusLocation,
		CancellationToken cancellationToken,
		Action<string, RouterParameterRequestStatusIdentifier>? trackTransaction = null)
	{
		ManagementTransactionUiRecipient? recipient = null;
		if (this.ControllerContext.HttpContext is { } httpContext &&
			!ManagementTransactionUiRecipient.TryCreate(
				httpContext,
				out recipient,
				out var validationError))
		{
			return this.BadRequest(validationError);
		}

		var statusIdentifier = await submit();
		if (trackTransaction is not null &&
			this.ControllerContext.HttpContext is { } browserHttpContext &&
			BrowserSessionIdentifier.TryGet(browserHttpContext, out var browserSessionIdentifier))
		{
			trackTransaction(
				browserSessionIdentifier,
				statusIdentifier);
		}

		if (recipient is not null)
		{
			await managementTransactions.RegisterUiRecipientAsync(
				statusIdentifier,
				recipient,
				cancellationToken);
		}

		return new SeeOtherRedirectResult(statusLocation(statusIdentifier));
	}

	private static RouterParameterTableSelection CreateTableSelection(
		string parameterTable,
		ParameterNumber parameterNumber,
		ushort firstEntry,
		ushort lastEntry,
		string? validationMessage = null)
	{
		return new RouterParameterTableSelection(
			parameterTable,
			parameterNumber.Value,
			firstEntry,
			lastEntry,
			RouterParameterTables.Supported,
			validationMessage);
	}

	private static bool TryCreateTableSelection(
		string parameterTable,
		ParameterNumber parameterNumber,
		ushort firstEntry,
		ushort lastEntry,
		out RouterParameterTableSelection selection,
		out RouterParameterTableDefinition table,
		out ParameterTable parameterTableValue)
	{
		parameterTableValue = null!;

		if (!ParameterTableRoute.TryParse(parameterTable, out var parsedParameterTable))
		{
			selection = CreateTableSelection(
				parameterTable,
				parameterNumber,
				firstEntry,
				lastEntry,
				"Parameter Table must be permanent, non-volatile, or current.");
			table = null!;
			return false;
		}

		parameterTableValue = parsedParameterTable ?? throw new InvalidOperationException(
			"A parsed Parameter Table must have a value.");

		if (!RouterParameterTables.TryGet(parameterNumber, out table))
		{
			selection = CreateTableSelection(
				parameterTable,
				parameterNumber,
				firstEntry,
				lastEntry,
				"Select a supported Router Parameter Table.");
			return false;
		}

		if (firstEntry == 0 || lastEntry < firstEntry)
		{
			selection = CreateTableSelection(
				parameterTable,
				parameterNumber,
				firstEntry,
				lastEntry,
				"First entry must be at least 1 and no greater than last entry.");
			return false;
		}

		selection = CreateTableSelection(parameterTable, parameterNumber, firstEntry, lastEntry);
		return true;
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
		var pstnTableEntries = status is ReceivedRouterParameterRequestStatus receivedPstnTable
			? FormatPstnTableEntries(parameterNumber, receivedPstnTable.ParameterValue)
			: null;
		var wanTableEntries = status is ReceivedRouterParameterRequestStatus receivedWanTable
			? FormatWanTableEntries(parameterNumber, receivedWanTable.ParameterValue)
			: null;
		var lanTableEntries = status is ReceivedRouterParameterRequestStatus receivedLanTable
			? FormatLanTableEntries(parameterNumber, receivedLanTable.ParameterValue)
			: null;
		var isdnTableEntries = status is ReceivedRouterParameterRequestStatus receivedIsdnTable
			? FormatIsdnTableEntries(parameterNumber, receivedIsdnTable.ParameterValue)
			: null;
		var mdtTableEntries = status is ReceivedRouterParameterRequestStatus receivedMdtTable
			? FormatMdtTableEntries(parameterNumber, receivedMdtTable.ParameterValue)
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
			pstnTableEntries,
			moreValues,
			rejectionReason)
		{
			WanTableEntries = wanTableEntries,
			LanTableEntries = lanTableEntries,
			IsdnTableEntries = isdnTableEntries,
			MdtTableEntries = mdtTableEntries
		};
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

	private static IReadOnlyList<PstnTableEntryStatusResponse>? FormatPstnTableEntries(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		if (parameterNumber != PstnTableParameterNumber)
		{
			return null;
		}

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var pstnTable = PstnTable.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router PSTN Table Parameter contains trailing encoded data.",
				nameof(parameterValue));
		}

		return pstnTable.Entries
			.Select(entry => new PstnTableEntryStatusResponse(
				entry.Index.Value,
				entry.Used.Value,
				Format(entry.NextNode),
				entry.TelephoneNumber.Value.Value,
				entry.HoldTime.Value,
				entry.Available.Value))
			.ToArray();
	}

	private static IReadOnlyList<WanTableEntryStatusResponse>? FormatWanTableEntries(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		if (parameterNumber != WanTableParameterNumber)
		{
			return null;
		}

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var wanTable = WanTable.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router WAN Table Parameter contains trailing encoded data.",
				nameof(parameterValue));
		}

		return wanTable.Entries
			.Select(entry => new WanTableEntryStatusResponse(
				entry.Index.Value,
				entry.Used.Value,
				Format(entry.NextNode),
				entry.WanAddress.Value.Value,
				FormatReasonCode(entry.ConnectType.Value)))
			.ToArray();
	}

	private static IReadOnlyList<LanTableEntryStatusResponse>? FormatLanTableEntries(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		if (parameterNumber != LanTableParameterNumber)
		{
			return null;
		}

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var lanTable = LanTable.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router LAN Table Parameter contains trailing encoded data.",
				nameof(parameterValue));
		}

		return lanTable.Entries
			.Select(entry => new LanTableEntryStatusResponse(
				entry.Index.Value,
				entry.Used.Value,
				Format(entry.NextNode),
				entry.LanAddress.Value.Value))
			.ToArray();
	}

	private static IReadOnlyList<IsdnTableEntryStatusResponse>? FormatIsdnTableEntries(
		ParameterNumber? parameterNumber,
		ParameterValue parameterValue)
	{
		if (parameterNumber != IsdnTableParameterNumber)
		{
			return null;
		}

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var isdnTable = IsdnTable.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router ISDN Table Parameter contains trailing encoded data.",
				nameof(parameterValue));
		}

		return isdnTable.Entries
			.Select(entry => new IsdnTableEntryStatusResponse(
				entry.Index.Value, entry.Used.Value, Format(entry.NextNode),
				entry.TelephoneNumber.Value.Value, entry.HoldTime.Value, entry.Available.Value))
			.ToArray();
	}

	private static IReadOnlyList<MdtTableEntryStatusResponse>? FormatMdtTableEntries(
		ParameterNumber? parameterNumber, ParameterValue parameterValue)
	{
		if (parameterNumber != MdtTableParameterNumber)
		{
			return null;
		}

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var mdtTable = MdtTable.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Router MDT Table Parameter contains trailing encoded data.",
				nameof(parameterValue));
		}

		return mdtTable.Entries.Select(entry => new MdtTableEntryStatusResponse(
			entry.Index.Value, entry.Used.Value, Format(entry.NextNode),
			entry.NetworkUserAddress.Value.Value, entry.HoldTime.Value, entry.Available.Value)).ToArray();
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
