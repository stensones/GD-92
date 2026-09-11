using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ActivatePeripheral : IGD92MessageContents
{
	private static readonly MessageType ActivatePeripheralMessageType =
		MessageType.FromValue(GD92MessageType.ActivatePeripheral);

	private ActivatePeripheral(OutputPeripherals outputPeripherals)
	{
		this.OutputPeripherals = outputPeripherals;
	}

	public OutputPeripherals OutputPeripherals { get; }
	public MessageType Type => ActivatePeripheralMessageType;

	public static ActivatePeripheral FromFields(OutputPeripherals outputPeripherals)
	{
		ArgumentNullException.ThrowIfNull(outputPeripherals);

		return new ActivatePeripheral(outputPeripherals);
	}

	public static ActivatePeripheral FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(OutputPeripherals.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.OutputPeripherals.ToWireValue();
	}
}
