namespace Stensones.GD92.Fields;

public readonly record struct ProtocolBoolean
{
	private ProtocolBoolean(byte value)
	{
		this.Value = value;
	}

	public static ProtocolBoolean False { get; } = new(0);
	public static ProtocolBoolean True { get; } = new(1);

	public byte Value { get; }

	public static ProtocolBoolean FromValue(byte value)
	{
		return value switch
		{
			0 => False,
			1 => True,
			_ => throw new ArgumentOutOfRangeException(nameof(value))
		};
	}
}
