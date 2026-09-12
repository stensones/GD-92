namespace Stensones.GD92.Fields;

public sealed record RoutingTable : UncountedTableField<RoutingTableEntry>, IGD9Field
{
	private const int MaximumEntryCount = 200;

	private RoutingTable(IReadOnlyList<RoutingTableEntry> entries)
		: base(entries)
	{
	}

	public static RoutingTable FromEntries(params RoutingTableEntry[] entries)
	{
		return new RoutingTable(ValidateEntries(entries, MaximumEntryCount));
	}

	public static RoutingTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var entries = new List<RoutingTableEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			entries.Add(RoutingTableEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromEntries(entries.ToArray());
	}
}
