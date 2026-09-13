using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

[Route("participants")]
public sealed class ParticipantParametersController(
	RouterParameterRequestSettings settings,
	IParticipantParameterRequestService participantParameterRequests,
	ManagementTransactions managementTransactions) : Controller
{
	[HttpPost("{port}/parameters/{parameterTable}/{parameterNumber}")]
	public async Task<IActionResult> RequestParameter(
		byte port,
		string parameterTable,
		byte parameterNumber,
		CancellationToken cancellationToken)
	{
		if (!ParameterTableRoute.TryParse(parameterTable, out var table))
		{
			return this.BadRequest("Parameter Table must be permanent, non-volatile, or current.");
		}

		var destination = CommunicationsAddress.FromValues(
			settings.MessageOriginator.Brigade,
			settings.MessageOriginator.Node,
			Port.FromValue(PortIdentifier.FromValue(port)));
		var statusIdentifier = await participantParameterRequests.RequestParameter(
			destination,
			table,
			ParameterNumber.FromValue(parameterNumber),
			cancellationToken);

		return new SeeOtherRedirectResult(
			$"/participants/{port}/parameters/{parameterTable}/{parameterNumber}/status/{statusIdentifier}");
	}

	[HttpGet("{port}/parameters/{parameterTable}/{parameterNumber}/status/{identifier}")]
	public IActionResult ParameterStatus(byte port, string parameterTable, byte parameterNumber, string identifier)
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

		var number = ParameterNumber.FromValue(parameterNumber);
		return this.Ok(new ParticipantParameterRequestStatusResponse(
			status.Identifier.ToString(),
			ManagementTransactionStatusFormatter.GetState(status),
			number.Value,
			status is ReceivedRouterParameterRequestStatus received
				? FormatParameterValue(Port.FromValue(PortIdentifier.FromValue(port)), number, received.ParameterValue)
				: null));
	}

	private static string? FormatParameterValue(
		Port port,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue)
	{
		return port.Value switch
		{
			1 => FormatLanMtaParameterValue(parameterNumber, parameterValue),
			2 => FormatPrinterUaParameterValue(parameterNumber, parameterValue),
			25 => FormatNetworkManagementUaParameterValue(parameterNumber, parameterValue),
			_ => null
		};
	}

	private static string? FormatLanMtaParameterValue(
		ParameterNumber parameterNumber,
		ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = parameterNumber.Value switch
		{
			1 => Word8.FromEncodedMessageBuffer(ref buffer).Value.ToString(CultureInfo.InvariantCulture),
			2 => FormatLanMtaAgentType(AgentType.FromEncodedMessageBuffer(ref buffer)),
			3 => MtaStatus.FromEncodedMessageBuffer(ref buffer).Value.ToString(),
			4 => ProtocolBoolean.FromEncodedMessageBuffer(ref buffer).Value.ToString().ToLowerInvariant(),
			5 => FrameTransmitCount.FromEncodedMessageBuffer(ref buffer).Value
				.ToString(CultureInfo.InvariantCulture),
			6 => FrameReceiveCount.FromEncodedMessageBuffer(ref buffer).Value
				.ToString(CultureInfo.InvariantCulture),
			7 => FrameTransmitFailureCount.FromEncodedMessageBuffer(ref buffer).Value
				.ToString(CultureInfo.InvariantCulture),
			8 => FrameReceiveFailureCount.FromEncodedMessageBuffer(ref buffer).Value
				.ToString(CultureInfo.InvariantCulture),
			9 => MtaMinimumMessagePriority.FromEncodedMessageBuffer(ref buffer).Value.Value
				.ToString(CultureInfo.InvariantCulture),
			10 => $"{DestinationNodes.FromEncodedMessageBuffer(ref buffer).AddressRanges.Count} destinations",
			21 => LanAddress.FromEncodedMessageBuffer(ref buffer).Value.Value,
			_ => null
		};
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"LAN MTA Parameter {parameterNumber.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}

		return value;
	}

	private static string? FormatPrinterUaParameterValue(
		ParameterNumber parameterNumber,
		ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = parameterNumber.Value switch
		{
			1 => Word8.FromEncodedMessageBuffer(ref buffer).Value.ToString(CultureInfo.InvariantCulture),
			2 => FormatPrinterUaAgentType(AgentType.FromEncodedMessageBuffer(ref buffer)),
			3 => FormatAddress(CommunicationsAddress.FromEncodedMessageBuffer(ref buffer)),
			21 => FormatAddressRange(AddressRange.FromEncodedMessageBuffer(ref buffer)),
			22 or 24 => ProtocolBoolean.FromEncodedMessageBuffer(ref buffer).Value
				.ToString().ToLowerInvariant(),
			23 => $"{AddressTable.FromEncodedMessageBuffer(ref buffer).Entries.Count} entries",
			_ => null
		};
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Printer UA Parameter {parameterNumber.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}

		return value;
	}

	private static string? FormatNetworkManagementUaParameterValue(
		ParameterNumber parameterNumber,
		ParameterValue parameterValue)
	{
		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var value = parameterNumber.Value switch
		{
			1 => Word8.FromEncodedMessageBuffer(ref buffer).Value.ToString(CultureInfo.InvariantCulture),
			2 => FormatNetworkManagementUaAgentType(AgentType.FromEncodedMessageBuffer(ref buffer)),
			3 => FormatAddress(CommunicationsAddress.FromEncodedMessageBuffer(ref buffer)),
			_ => null
		};
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				$"Network Management UA Parameter {parameterNumber.Value} contains trailing encoded data.",
				nameof(parameterValue));
		}

		return value;
	}

	private static string FormatLanMtaAgentType(AgentType agentType)
	{
		return agentType.Value == AgentTypeValue.LanMessageTransferAgent
			? "LAN MTA (10)"
			: $"{agentType.Value} ({(byte)agentType.Value})";
	}

	private static string FormatPrinterUaAgentType(AgentType agentType)
	{
		return agentType.Value == AgentTypeValue.Printer
			? "Printer (4)"
			: $"{agentType.Value} ({(byte)agentType.Value})";
	}

	private static string FormatNetworkManagementUaAgentType(AgentType agentType)
	{
		return agentType.Value == AgentTypeValue.NetworkManagementUserAgent
			? "Network Management UA (12)"
			: $"{agentType.Value} ({(byte)agentType.Value})";
	}

	private static string FormatAddressRange(AddressRange addressRange)
	{
		return $"{FormatAddress(addressRange.FirstAddress)}-{FormatAddress(addressRange.LastAddress)}";
	}

	private static string FormatAddress(CommunicationsAddress address)
	{
		return string.Join(
			'.',
			address.Brigade.Value.ToString(),
			address.Node.Value.ToString(CultureInfo.InvariantCulture),
			address.Port.Value.ToString(CultureInfo.InvariantCulture));
	}
}
