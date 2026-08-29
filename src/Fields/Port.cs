namespace Stensones.GD92.Fields;

public sealed record Port
{
	private Port(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static Port FromValue(byte value)
	{
		if (value > 63)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new Port(value);
	}
}
