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
	[HttpPost("{port}/parameters/current/{parameterNumber}")]
	public async Task<IActionResult> RequestCurrentParameter(
		byte port,
		byte parameterNumber,
		CancellationToken cancellationToken)
	{
		var destination = CommunicationsAddress.FromValues(
			settings.MessageOriginator.Brigade,
			settings.MessageOriginator.Node,
			Port.FromValue(PortIdentifier.FromValue(port)));
		var statusIdentifier = await participantParameterRequests.RequestCurrentParameter(
			destination,
			ParameterNumber.FromValue(parameterNumber),
			cancellationToken);

		return new SeeOtherRedirectResult(
			$"/participants/{port}/parameters/current/{parameterNumber}/status/{statusIdentifier}");
	}

	[HttpGet("{port}/parameters/current/{parameterNumber}/status/{identifier}")]
	public IActionResult CurrentStatus(byte port, byte parameterNumber, string identifier)
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
		if (port.Value != 1)
		{
			return null;
		}

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

	private static string FormatLanMtaAgentType(AgentType agentType)
	{
		return agentType.Value == AgentTypeValue.LanMessageTransferAgent
			? "LAN MTA (10)"
			: $"{agentType.Value} ({(byte)agentType.Value})";
	}
}
