namespace Stensones.GD92.Fields;

public sealed record DestinationCount
{
	private DestinationCount(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static DestinationCount FromValue(DestinationAddressCount value)
	{
		return new DestinationCount(value.Value);
	}
}
