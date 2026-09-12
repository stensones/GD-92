using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

internal static class MessageContentsCodec
{
	private const string ContentsLengthMismatchMessage =
		"The encoded Message Contents length does not match its Message Type.";

	public static IGD92MessageContents Decode(
		MessageType messageType,
		ReadOnlySpan<byte> contentsWireValue)
	{
		ArgumentNullException.ThrowIfNull(messageType);

		var contentsBuffer = new EncodedMessageBuffer(contentsWireValue);
		IGD92MessageContents contents = (GD92MessageType)messageType.Value switch
		{
			GD92MessageType.MobiliseCommand => MobiliseCommand.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.MobiliseMessage => MobiliseMessage.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.PageOfficer => PageOfficer.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.AreaPageMessage => AreaPageMessage.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ActivatePeripheral => ActivatePeripheral.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.DeactivatePeripheral => DeactivatePeripheral.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ResourceStatusRequest => ResourceStatusRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.PeripheralStatusRequest => PeripheralStatusRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ResourceStatus => ResourceStatus.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.DutyStaffingUpdate => DutyStaffingUpdate.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.LogUpdate => LogUpdate.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Stop => Stop.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.MakeUp => MakeUp.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.PeripheralStatus => PeripheralStatus.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.IncidentNotification => IncidentNotification.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.AlertCrew => AlertCrew.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.AlertStatus => AlertStatus.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.AlertEng => AlertEng.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Test => Test.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.PrinterStatus => PrinterStatus.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.MtaStatusChange => MtaStatusChange.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.RouteStatus => RouteStatus.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.BrigadeMessage => BrigadeMessage.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.DatabaseQuery => DataBaseQuery.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.FormattedText => FormattedText.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ProformaDefinitionQuery => ProformaDefinitionQuery.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ProformaDefinition => ProformaDefinition.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.InterruptRequest => InterruptRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ResetRequest => ResetRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Reset => Reset.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Text => Text.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Acknowledgement => Acknowledgement.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.NegativeAcknowledgement => NegativeAcknowledgement.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.SetParameter => SetParameter.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ParameterRequest => ParameterRequest.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.Parameter => Parameter.FromEncodedMessageBuffer(ref contentsBuffer),
			GD92MessageType.ParameterRequestMultiple => ParameterRequestMultiple.FromEncodedMessageBuffer(ref contentsBuffer),
			_ => UnsupportedMessageContents.FromWireValue(messageType, contentsWireValue)
		};

		if (contents is not UnsupportedMessageContents && contentsBuffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException(ContentsLengthMismatchMessage);
		}

		return contents;
	}
}
