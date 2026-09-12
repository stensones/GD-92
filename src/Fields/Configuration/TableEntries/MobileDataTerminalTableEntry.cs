namespace Stensones.GD92.Fields;

public sealed record MobileDataTerminalTableEntry : IUncountedTableEntry
{
	private const int BooleanBitCount = 8;

	private MobileDataTerminalTableEntry(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		NetworkUserAddress networkUserAddress,
		HoldTime holdTime,
		ProtocolBoolean available)
	{
		this.Index = index;
		this.Used = used;
		this.NextNode = nextNode;
		this.NetworkUserAddress = networkUserAddress;
		this.HoldTime = holdTime;
		this.Available = available;
	}

	public ParameterEntryIndex Index { get; }
	public ProtocolBoolean Used { get; }
	public CommunicationsAddress NextNode { get; }
	public NetworkUserAddress NetworkUserAddress { get; }
	public HoldTime HoldTime { get; }
	public ProtocolBoolean Available { get; }

	public static MobileDataTerminalTableEntry FromValues(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		NetworkUserAddress networkUserAddress,
		HoldTime holdTime,
		ProtocolBoolean available)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(nextNode);
		ArgumentNullException.ThrowIfNull(networkUserAddress);
		ArgumentNullException.ThrowIfNull(holdTime);

		return new MobileDataTerminalTableEntry(
			index,
			used,
			nextNode,
			networkUserAddress,
			holdTime,
			available);
	}

	public static MobileDataTerminalTableEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromValue((byte)buffer.ReadUnsignedBits(BooleanBitCount)),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer),
			NetworkUserAddress.FromEncodedMessageBuffer(ref buffer),
			HoldTime.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromValue((byte)buffer.ReadUnsignedBits(BooleanBitCount)));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			this.Used.Value,
			.. this.NextNode.ToWireValue(),
			.. this.NetworkUserAddress.ToWireValue(),
			.. this.HoldTime.ToWireValue(),
			this.Available.Value
		];
	}
}
