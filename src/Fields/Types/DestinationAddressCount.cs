namespace Stensones.GD92.Fields;

public readonly record struct DestinationAddressCount
{
	private const byte MinimumValue = 1;
	private const byte MaximumValue = 63;

	private DestinationAddressCount(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static DestinationAddressCount FromValue(byte value)
	{
		if (value is < MinimumValue or > MaximumValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new DestinationAddressCount(value);
	}
}
