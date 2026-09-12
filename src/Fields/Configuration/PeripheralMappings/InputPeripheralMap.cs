namespace Stensones.GD92.Fields;

public sealed record InputPeripheralMap : IGD9Field
{
	private InputPeripheralMap(
		PhysicalBit physicalBit,
		ActiveState activeState,
		GenerateAlarm generateAlarm)
	{
		this.PhysicalBit = physicalBit;
		this.ActiveState = activeState;
		this.GenerateAlarm = generateAlarm;
	}

	public PhysicalBit PhysicalBit { get; }
	public ActiveState ActiveState { get; }
	public GenerateAlarm GenerateAlarm { get; }

	public static InputPeripheralMap FromValues(
		PhysicalBit physicalBit,
		ActiveState activeState,
		GenerateAlarm generateAlarm)
	{
		ArgumentNullException.ThrowIfNull(physicalBit);
		ArgumentNullException.ThrowIfNull(activeState);
		ArgumentNullException.ThrowIfNull(generateAlarm);

		return new InputPeripheralMap(physicalBit, activeState, generateAlarm);
	}

	public static InputPeripheralMap FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			PhysicalBit.FromEncodedMessageBuffer(ref buffer),
			ActiveState.FromEncodedMessageBuffer(ref buffer),
			GenerateAlarm.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.PhysicalBit.ToWireValue(),
			.. this.ActiveState.ToWireValue(),
			.. this.GenerateAlarm.ToWireValue()
		];
	}
}
