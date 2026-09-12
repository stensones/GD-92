namespace Stensones.GD92.Fields;

public sealed record TelephoneStatistics : UncountedTableField<TelephoneStatisticsEntry>, IGD9Field
{
	private TelephoneStatistics(IReadOnlyList<TelephoneStatisticsEntry> entries)
		: base(entries)
	{
	}

	public static TelephoneStatistics FromEntries(params TelephoneStatisticsEntry[] entries)
	{
		return new TelephoneStatistics(ValidateEntries(entries, int.MaxValue));
	}

	public static TelephoneStatistics FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var entries = new List<TelephoneStatisticsEntry>();

		while (buffer.RemainingBitCount > 0)
		{
			entries.Add(TelephoneStatisticsEntry.FromEncodedMessageBuffer(ref buffer));
		}

		return FromEntries(entries.ToArray());
	}
}
