namespace Stensones.GD92.Fields;

public sealed record LanTable : UncountedTableField<LanTableEntry>, IGD9Field
{
	private const int MaximumEntryCount = 200;

	private LanTable(IReadOnlyList<LanTableEntry> entries)
		: base(entries)
	{
	}

	public static LanTable FromEntries(params LanTableEntry[] entries)
	{
		return new LanTable(ValidateEntries(entries, MaximumEntryCount));
	}

	public static LanTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new LanTable(DecodeEntries(ref buffer, MaximumEntryCount));
	}
}
