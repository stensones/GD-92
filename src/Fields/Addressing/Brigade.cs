namespace Stensones.GD92.Fields;

public sealed record Brigade
{
	private Brigade(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static Brigade FromValue(byte value)
	{
		return new Brigade(value);
	}
}
