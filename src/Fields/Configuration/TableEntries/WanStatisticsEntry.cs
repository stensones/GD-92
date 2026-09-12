namespace Stensones.GD92.Fields;

public sealed record WanStatisticsEntry : IGD9Field
{
	private WanStatisticsEntry(
		ParameterEntryIndex index,
		TimeAndDate timeAndDate,
		WanAddress wanAddress,
		ConnectTime connectTime)
	{
		this.Index = index;
		this.TimeAndDate = timeAndDate;
		this.WanAddress = wanAddress;
		this.ConnectTime = connectTime;
	}

	public ParameterEntryIndex Index { get; }
	public TimeAndDate TimeAndDate { get; }
	public WanAddress WanAddress { get; }
	public ConnectTime ConnectTime { get; }

	public static WanStatisticsEntry FromValues(
		ParameterEntryIndex index,
		TimeAndDate timeAndDate,
		WanAddress wanAddress,
		ConnectTime connectTime)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(timeAndDate);
		ArgumentNullException.ThrowIfNull(wanAddress);
		ArgumentNullException.ThrowIfNull(connectTime);

		return new WanStatisticsEntry(index, timeAndDate, wanAddress, connectTime);
	}

	public static WanStatisticsEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			TimeAndDate.FromEncodedMessageBuffer(ref buffer),
			WanAddress.FromEncodedMessageBuffer(ref buffer),
			ConnectTime.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			.. this.TimeAndDate.ToWireValue(),
			.. this.WanAddress.ToWireValue(),
			.. this.ConnectTime.ToWireValue()
		];
	}
}
