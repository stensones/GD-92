namespace Stensones.GD92.Fields;

public sealed record PstnTable : UncountedTableField<TelephoneTableEntry>, IGD9Field
{
	private const int MaximumEntryCount = 200;

	private PstnTable(IReadOnlyList<TelephoneTableEntry> entries)
		: base(entries)
	{
	}

	public static PstnTable FromEntries(params TelephoneTableEntry[] entries)
	{
		return new PstnTable(ValidateEntries(entries, MaximumEntryCount));
	}

	public static PstnTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new PstnTable(DecodeEntries(ref buffer, MaximumEntryCount));
	}
}
