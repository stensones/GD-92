namespace Stensones.GD92.Fields;

public sealed record DestinationCount
{
	private DestinationCount(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static DestinationCount FromValue(byte value)
	{
		if (value is < 1 or > 63)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new DestinationCount(value);
	}
}
