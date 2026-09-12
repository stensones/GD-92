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
		return new WanTable(DecodeEntries(ref buffer, int.MaxValue));
	}
}
