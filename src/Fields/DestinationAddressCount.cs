namespace Stensones.GD92.Fields;

public readonly record struct DestinationAddressCount
{
	private DestinationAddressCount(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static DestinationAddressCount FromValue(byte value)
	{
		if (value is < 1 or > 63)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new DestinationAddressCount(value);
	}
}
