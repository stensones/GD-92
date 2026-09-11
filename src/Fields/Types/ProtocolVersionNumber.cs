namespace Stensones.GD92.Fields;

public readonly record struct ProtocolVersionNumber
{
	private const byte MinimumValue = 1;
	private const byte MaximumValue = 15;

	private ProtocolVersionNumber(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static ProtocolVersionNumber FromValue(byte value)
	{
		if (value is < MinimumValue or > MaximumValue)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ProtocolVersionNumber(value);
	}
}
