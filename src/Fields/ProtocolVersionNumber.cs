namespace Stensones.GD92.Fields;

public readonly record struct ProtocolVersionNumber
{
	private ProtocolVersionNumber(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static ProtocolVersionNumber FromValue(byte value)
	{
		if (value is < 1 or > 15)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ProtocolVersionNumber(value);
	}
}
