namespace Stensones.GD92.Fields;

public readonly record struct ParameterTableIdentifier
{
	private const byte PermanentValue = 0;
	private const byte NonVolatileValue = 1;
	private const byte CurrentValue = 2;

	private ParameterTableIdentifier(byte value)
	{
		this.Value = value;
	}

	public static ParameterTableIdentifier Permanent { get; } = new(PermanentValue);
	public static ParameterTableIdentifier NonVolatile { get; } = new(NonVolatileValue);
	public static ParameterTableIdentifier Current { get; } = new(CurrentValue);

	public byte Value { get; }

	public static ParameterTableIdentifier FromValue(byte value)
	{
		return value switch
		{
			PermanentValue => Permanent,
			NonVolatileValue => NonVolatile,
			CurrentValue => Current,
			_ => throw new ArgumentOutOfRangeException(nameof(value))
		};
	}
}
