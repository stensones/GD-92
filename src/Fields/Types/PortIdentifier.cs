namespace Stensones.GD92.Fields;

public readonly record struct PortIdentifier
{
	private const byte MaximumValue = 63;

	private PortIdentifier(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static PortIdentifier FromValue(byte value)
	{
		if (value > MaximumValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new PortIdentifier(value);
	}
}
