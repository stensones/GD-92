namespace Stensones.GD92.Fields;

public sealed record WanStatistics : UncountedTableField<WanStatisticsEntry>, IGD9Field
{
	private WanStatistics(IReadOnlyList<WanStatisticsEntry> entries)
		: base(entries)
	{
	}

	public static WanStatistics FromEntries(params WanStatisticsEntry[] entries)
	{
		return new WanStatistics(ValidateEntries(entries, int.MaxValue));
	}

	public static WanStatistics FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return new WanStatistics(DecodeEntries(ref buffer, int.MaxValue));
	}
}
