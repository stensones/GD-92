namespace Stensones.GD92.Fields;

public readonly record struct PortIdentifier
{
	private PortIdentifier(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static PortIdentifier FromValue(byte value)
	{
		if (value > 63)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new PortIdentifier(value);
	}
}
