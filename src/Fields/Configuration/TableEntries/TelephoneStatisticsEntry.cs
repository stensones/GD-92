namespace Stensones.GD92.Fields;

public sealed record TelephoneStatisticsEntry : IGD9Field
{
	private TelephoneStatisticsEntry(
		ParameterEntryIndex index,
		TimeAndDate timeAndDate,
		TelephoneNumber telephoneNumber,
		ConnectTime connectTime)
	{
		this.Index = index;
		this.TimeAndDate = timeAndDate;
		this.TelephoneNumber = telephoneNumber;
		this.ConnectTime = connectTime;
	}

	public ParameterEntryIndex Index { get; }
	public TimeAndDate TimeAndDate { get; }
	public TelephoneNumber TelephoneNumber { get; }
	public ConnectTime ConnectTime { get; }

	public static TelephoneStatisticsEntry FromValues(
		ParameterEntryIndex index,
		TimeAndDate timeAndDate,
		TelephoneNumber telephoneNumber,
		ConnectTime connectTime)
	{
		ArgumentNullException.ThrowIfNull(index);
		ArgumentNullException.ThrowIfNull(timeAndDate);
		ArgumentNullException.ThrowIfNull(telephoneNumber);
		ArgumentNullException.ThrowIfNull(connectTime);

		return new TelephoneStatisticsEntry(index, timeAndDate, telephoneNumber, connectTime);
	}

	public static TelephoneStatisticsEntry FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			TimeAndDate.FromEncodedMessageBuffer(ref buffer),
			TelephoneNumber.FromEncodedMessageBuffer(ref buffer),
			ConnectTime.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Index.ToWireValue(),
			.. this.TimeAndDate.ToWireValue(),
			.. this.TelephoneNumber.ToWireValue(),
			.. this.ConnectTime.ToWireValue()
		];
	}
}
