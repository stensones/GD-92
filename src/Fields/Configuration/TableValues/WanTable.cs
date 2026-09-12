namespace Stensones.GD92.Fields;

public sealed record WanTable : UncountedTableField<WanTableEntry>, IGD9Field
{
	private WanTable(IReadOnlyList<WanTableEntry> entries)
		: base(entries)
	{
	}

	public static WanTable FromEntries(params WanTableEntry[] entries)
	{
		return new WanTable(ValidateEntries(entries, int.MaxValue));
	}

	public static WanTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var entries = new List<WanTableEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			entries.Add(WanTableEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromEntries(entries.ToArray());
	}
}
