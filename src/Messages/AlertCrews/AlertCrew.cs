using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record AlertCrew : IGD92MessageContents
{
	private static readonly MessageType AlertCrewMessageType =
		MessageType.FromValue(GD92MessageType.AlertCrew);

	private AlertCrew(
		AlertGroup alertGroup,
		ManualAcknowledgementRequest manualAcknowledgementRequest,
		OutputPeripherals outputPeripherals)
	{
		this.AlertGroup = alertGroup;
		this.ManualAcknowledgementRequest = manualAcknowledgementRequest;
		this.OutputPeripherals = outputPeripherals;
	}

	public AlertGroup AlertGroup { get; }
	public ManualAcknowledgementRequest ManualAcknowledgementRequest { get; }
	public OutputPeripherals OutputPeripherals { get; }
	public MessageType Type => AlertCrewMessageType;

	public static AlertCrew FromFields(
		AlertGroup alertGroup,
		ManualAcknowledgementRequest manualAcknowledgementRequest,
		OutputPeripherals outputPeripherals)
	{
		ArgumentNullException.ThrowIfNull(alertGroup);
		ArgumentNullException.ThrowIfNull(manualAcknowledgementRequest);
		ArgumentNullException.ThrowIfNull(outputPeripherals);

		return new AlertCrew(alertGroup, manualAcknowledgementRequest, outputPeripherals);
	}

	public static AlertCrew FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			AlertGroup.FromEncodedMessageBuffer(ref buffer),
			ManualAcknowledgementRequest.FromEncodedMessageBuffer(ref buffer),
			OutputPeripherals.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.AlertGroup.ToWireValue(),
			.. this.ManualAcknowledgementRequest.ToWireValue(),
			.. this.OutputPeripherals.ToWireValue()
		];
	}
}
