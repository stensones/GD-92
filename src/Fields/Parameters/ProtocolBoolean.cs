namespace Stensones.GD92.Fields;

public readonly record struct ProtocolBoolean
{
	private const byte FalseValue = 0;
	private const byte TrueValue = 1;

	private ProtocolBoolean(byte value)
	{
		this.Value = value;
	}

	public static ProtocolBoolean False { get; } = new(FalseValue);
	public static ProtocolBoolean True { get; } = new(TrueValue);

	public byte Value { get; }

	public static ProtocolBoolean FromValue(byte value)
	{
		return value switch
		{
			FalseValue => False,
			TrueValue => True,
			_ => throw new ArgumentOutOfRangeException(nameof(value))
		};
	}
}
