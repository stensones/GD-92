namespace Stensones.GD92.Fields;

public sealed record MdtTable : UncountedTableField<MobileDataTerminalTableEntry>, IGD9Field
{
	private const int MaximumEntryCount = 200;

	private MdtTable(IReadOnlyList<MobileDataTerminalTableEntry> entries)
		: base(entries)
	{
	}

	public static MdtTable FromEntries(params MobileDataTerminalTableEntry[] entries)
	{
		return new MdtTable(ValidateEntries(entries, MaximumEntryCount));
	}

	public static MdtTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var entries = new List<MobileDataTerminalTableEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			entries.Add(MobileDataTerminalTableEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromEntries(entries.ToArray());
	}
}
