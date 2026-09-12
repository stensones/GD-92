namespace Stensones.GD92.Fields;

public sealed record RoutingTableEntry : IUncountedTableEntry
{
	private const int BooleanBitCount = 8;

	private RoutingTableEntry(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		DestinationNodes destinationNodes,
		AgentType agentType,
		RoutingPreference preference)
	{
		this.Index = index;
		this.Used = used;
		this.NextNode = nextNode;
		this.DestinationNodes = destinationNodes;
		this.AgentType = agentType;
		this.Preference = preference;
	}

	public ParameterEntryIndex Index { get; }
	public ProtocolBoolean Used { get; }
	public CommunicationsAddress NextNode { get; }
	public DestinationNodes DestinationNodes { get; }
	public AgentType AgentType { get; }
	public RoutingPreference Preference { get; }

	public static RoutingTableEntry FromValues(
		ParameterEntryIndex index,
		ProtocolBoolean used,
		CommunicationsAddress nextNode,
		DestinationNodes destinationNodes,
		AgentType agentType,
		RoutingPreference preference)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(nextNode);
		ArgumentNullException.ThrowIfNull(destinationNodes);
		ArgumentNullException.ThrowIfNull(agentType);
		ArgumentNullException.ThrowIfNull(preference);

		return new RoutingTableEntry(index, used, nextNode, destinationNodes, agentType, preference);
	}

	public static RoutingTableEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			ProtocolBoolean.FromValue((byte)buffer.ReadUnsignedBits(BooleanBitCount)),
			CommunicationsAddress.FromEncodedMessageBuffer(ref buffer),
			DestinationNodes.FromEncodedMessageBuffer(ref buffer),
			AgentType.FromEncodedMessageBuffer(ref buffer),
			RoutingPreference.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			this.Used.Value,
			.. this.NextNode.ToWireValue(),
			.. this.DestinationNodes.ToWireValue(),
			.. this.AgentType.ToWireValue(),
			.. this.Preference.ToWireValue()
		];
	}
}
