namespace Stensones.GD92.Fields;

public sealed record TelephoneTableEntry : IUncountedTableEntry
{
	private TelephoneTableEntry(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		TelephoneNumber telephoneNumber,
		HoldTime holdTime,
		ProtocolBoolean available)
	{
		this.Index = index;
		this.Used = used;
		this.NextNode = nextNode;
		this.TelephoneNumber = telephoneNumber;
		this.HoldTime = holdTime;
		this.Available = available;
	}

	public ParameterEntryIndex Index { get; }
	public ProtocolBoolean Used { get; }
	public CommunicationsAddress NextNode { get; }
	public TelephoneNumber TelephoneNumber { get; }
	public HoldTime HoldTime { get; }
	public ProtocolBoolean Available { get; }

	public static TelephoneTableEntry FromValues(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		TelephoneNumber telephoneNumber,
		HoldTime holdTime,
		ProtocolBoolean available)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(nextNode);
		ArgumentNullException.ThrowIfNull(telephoneNumber);
		ArgumentNullException.ThrowIfNull(holdTime);

		return new TelephoneTableEntry(index, used, nextNode, telephoneNumber, holdTime, available);
	}

	public static TelephoneTableEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromEncodedMessageBuffer(ref buffer),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer),
			TelephoneNumber.FromEncodedMessageBuffer(ref buffer),
			HoldTime.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			.. this.Used.ToWireValue(),
			.. this.NextNode.ToWireValue(),
			.. this.TelephoneNumber.ToWireValue(),
			.. this.HoldTime.ToWireValue(),
			.. this.Available.ToWireValue()
		];
	}
}
