namespace Stensones.GD92.Fields;

public sealed record AddressTable : UncountedTableField<AlternativeAddressTableEntry>, IGD9Field
{
	private const int MaximumEntryCount = 5;

	private AddressTable(IReadOnlyList<AlternativeAddressTableEntry> entries)
		: base(entries)
	{
	}

	public static AddressTable FromEntries(params AlternativeAddressTableEntry[] entries)
	{
		return new AddressTable(ValidateEntries(entries, MaximumEntryCount));
	}

	public static AddressTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new AddressTable(DecodeEntries(ref buffer, MaximumEntryCount));
	}
}
