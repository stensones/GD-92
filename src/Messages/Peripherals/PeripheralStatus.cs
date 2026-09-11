using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record PeripheralStatus : IGD92MessageContents
{
	private static readonly MessageType PeripheralStatusMessageType =
		MessageType.FromValue(GD92MessageType.PeripheralStatus);

	private PeripheralStatus(InputPeripherals inputPeripherals, OutputPeripherals outputPeripherals)
	{
		this.InputPeripherals = inputPeripherals;
		this.OutputPeripherals = outputPeripherals;
	}

	public InputPeripherals InputPeripherals { get; }
	public OutputPeripherals OutputPeripherals { get; }
	public MessageType Type => PeripheralStatusMessageType;

	public static PeripheralStatus FromFields(
		InputPeripherals inputPeripherals,
		OutputPeripherals outputPeripherals)
	{
		ArgumentNullException.ThrowIfNull(inputPeripherals);
		ArgumentNullException.ThrowIfNull(outputPeripherals);

		return new PeripheralStatus(inputPeripherals, outputPeripherals);
	}

	public static PeripheralStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			InputPeripherals.FromEncodedMessageBuffer(ref buffer),
			OutputPeripherals.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [.. this.InputPeripherals.ToWireValue(), .. this.OutputPeripherals.ToWireValue()];
	}
}
