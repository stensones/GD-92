namespace Stensones.GD92.Fields;

public sealed record OutputPeripheralMap : IGD9Field
{
	private OutputPeripheralMap(
		PhysicalBit physicalBit,
		ActiveState activeState,
		PulseLength pulseLength)
	{
		this.PhysicalBit = physicalBit;
		this.ActiveState = activeState;
		this.PulseLength = pulseLength;
	}

	public PhysicalBit PhysicalBit { get; }
	public ActiveState ActiveState { get; }
	public PulseLength PulseLength { get; }

	public static OutputPeripheralMap FromValues(
		PhysicalBit physicalBit,
		ActiveState activeState,
		PulseLength pulseLength)
	{
		ArgumentNullException.ThrowIfNull(physicalBit);
		ArgumentNullException.ThrowIfNull(activeState);
		ArgumentNullException.ThrowIfNull(pulseLength);

		return new OutputPeripheralMap(physicalBit, activeState, pulseLength);
	}

	public static OutputPeripheralMap FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			PhysicalBit.FromEncodedMessageBuffer(ref buffer),
			ActiveState.FromEncodedMessageBuffer(ref buffer),
			PulseLength.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.PhysicalBit.ToWireValue(),
			.. this.ActiveState.ToWireValue(),
			.. this.PulseLength.ToWireValue()
		];
	}
}
