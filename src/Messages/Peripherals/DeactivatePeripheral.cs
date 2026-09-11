using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record DeactivatePeripheral : IGD92MessageContents
{
	private static readonly MessageType DeactivatePeripheralMessageType =
		MessageType.FromValue(GD92MessageType.DeactivatePeripheral);

	private DeactivatePeripheral(OutputPeripherals outputPeripherals)
	{
		this.OutputPeripherals = outputPeripherals;
	}

	public OutputPeripherals OutputPeripherals { get; }
	public MessageType Type => DeactivatePeripheralMessageType;

	public static DeactivatePeripheral FromFields(OutputPeripherals outputPeripherals)
	{
		ArgumentNullException.ThrowIfNull(outputPeripherals);

		return new DeactivatePeripheral(outputPeripherals);
	}

	public static DeactivatePeripheral FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(OutputPeripherals.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.OutputPeripherals.ToWireValue();
	}
}
