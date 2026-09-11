using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record MobiliseCommand : IGD92MessageContents
{
	private static readonly MessageType MobiliseCommandMessageType =
		MessageType.FromValue(GD92MessageType.MobiliseCommand);

	private MobiliseCommand(
		OutputPeripherals outputPeripherals,
		ManualAcknowledgementRequest manualAcknowledgementRequest)
	{
		this.OutputPeripherals = outputPeripherals;
		this.ManualAcknowledgementRequest = manualAcknowledgementRequest;
	}

	public OutputPeripherals OutputPeripherals { get; }
	public ManualAcknowledgementRequest ManualAcknowledgementRequest { get; }
	public MessageType Type => MobiliseCommandMessageType;

	public static MobiliseCommand FromFields(
		OutputPeripherals outputPeripherals,
		ManualAcknowledgementRequest manualAcknowledgementRequest)
	{
		ArgumentNullException.ThrowIfNull(outputPeripherals);
		ArgumentNullException.ThrowIfNull(manualAcknowledgementRequest);

		return new MobiliseCommand(outputPeripherals, manualAcknowledgementRequest);
	}

	public static MobiliseCommand FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			OutputPeripherals.FromEncodedMessageBuffer(ref buffer),
			ManualAcknowledgementRequest.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.OutputPeripherals.ToWireValue(),
			.. this.ManualAcknowledgementRequest.ToWireValue()
		];
	}
}
