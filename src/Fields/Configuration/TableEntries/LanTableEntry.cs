namespace Stensones.GD92.Fields;

public sealed record LanTableEntry : IGD9Field
{
	private const int BooleanBitCount = 8;

	private LanTableEntry(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		LanAddress lanAddress)
	{
		this.Index = index;
		this.Used = used;
		this.NextNode = nextNode;
		this.LanAddress = lanAddress;
	}

	public ParameterEntryIndex Index { get; }
	public ProtocolBoolean Used { get; }
	public CommunicationsAddress NextNode { get; }
	public LanAddress LanAddress { get; }

	public static LanTableEntry FromValues(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		LanAddress lanAddress)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(nextNode);
		ArgumentNullException.ThrowIfNull(lanAddress);

		return new LanTableEntry(index, used, nextNode, lanAddress);
	}

	public static LanTableEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromValue((byte)buffer.ReadUnsignedBits(BooleanBitCount)),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer),
			LanAddress.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			this.Used.Value,
			.. this.NextNode.ToWireValue(),
			.. this.LanAddress.ToWireValue()
		];
	}
}
