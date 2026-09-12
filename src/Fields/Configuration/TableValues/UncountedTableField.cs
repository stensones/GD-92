namespace Stensones.GD92.Fields;

public interface IUncountedTableEntry : IGD9Field
{
	byte[] ToWireValue();
}

public abstract record UncountedTableField<TEntry>
	where TEntry : class, IUncountedTableEntry
{
	private const string NullTableEntryMessage = "Table entries may not contain null values.";

	protected UncountedTableField(IReadOnlyList<TEntry> entries)
	{
		this.Entries = entries;
	}

	public IReadOnlyList<TEntry> Entries { get; }

	public byte[] ToWireValue()
	{
		var wireValue = new List<byte>();

		foreach (var entry in this.Entries)
		{
			wireValue.AddRange(entry.ToWireValue());
		}

		return wireValue.ToArray();
	}

	protected static IReadOnlyList<TEntry> ValidateEntries(TEntry[] entries, int maximumEntryCount)
	{
		ArgumentNullException.ThrowIfNull(entries);

		ArgumentOutOfRangeException.ThrowIfLessThan(maximumEntryCount, entries.Length);

		if (entries.Any(entry => entry is null))
		{
			throw new ArgumentException(NullTableEntryMessage, nameof(entries));
		}

		return Array.AsReadOnly(entries.ToArray());
	}
}
