namespace Stensones.GD92.Fields;

public sealed record WanTableEntry : IUncountedTableEntry
{
	private WanTableEntry(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		WanAddress wanAddress,
		ConnectType connectType)
	{
		this.Index = index;
		this.Used = used;
		this.NextNode = nextNode;
		this.WanAddress = wanAddress;
		this.ConnectType = connectType;
	}

	public ParameterEntryIndex Index { get; }
	public ProtocolBoolean Used { get; }
	public CommunicationsAddress NextNode { get; }
	public WanAddress WanAddress { get; }
	public ConnectType ConnectType { get; }

	public static WanTableEntry FromValues(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		WanAddress wanAddress,
		ConnectType connectType)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(nextNode);
		ArgumentNullException.ThrowIfNull(wanAddress);
		ArgumentNullException.ThrowIfNull(connectType);

		return new WanTableEntry(index, used, nextNode, wanAddress, connectType);
	}

	public static WanTableEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromEncodedMessageBuffer(ref buffer),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer),
			WanAddress.FromEncodedMessageBuffer(ref buffer),
			ConnectType.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			.. this.Used.ToWireValue(),
			.. this.NextNode.ToWireValue(),
			.. this.WanAddress.ToWireValue(),
			.. this.ConnectType.ToWireValue()
		];
	}
}
