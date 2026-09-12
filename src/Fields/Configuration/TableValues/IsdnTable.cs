namespace Stensones.GD92.Fields;

public sealed record IsdnTable : UncountedTableField<TelephoneTableEntry>, IGD9Field
{
	private const int MaximumEntryCount = 200;

	private IsdnTable(IReadOnlyList<TelephoneTableEntry> entries)
		: base(entries)
	{
	}

	public static IsdnTable FromEntries(params TelephoneTableEntry[] entries)
	{
		return new IsdnTable(ValidateEntries(entries, MaximumEntryCount));
	}

	public static IsdnTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var entries = new List<TelephoneTableEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			entries.Add(TelephoneTableEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromEntries(entries.ToArray());
	}
}
