namespace Stensones.GD92.Fields;

public readonly record struct ParameterTableIdentifier
{
	private ParameterTableIdentifier(byte value)
	{
		this.Value = value;
	}

	public static ParameterTableIdentifier Permanent { get; } = new(0);
	public static ParameterTableIdentifier NonVolatile { get; } = new(1);
	public static ParameterTableIdentifier Current { get; } = new(2);

	public byte Value { get; }

	public static ParameterTableIdentifier FromValue(byte value)
	{
		return value switch
		{
			0 => Permanent,
			1 => NonVolatile,
			2 => Current,
			_ => throw new ArgumentOutOfRangeException(nameof(value))
		};
	}
}
