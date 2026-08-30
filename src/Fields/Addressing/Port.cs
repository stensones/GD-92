namespace Stensones.GD92.Fields;

public sealed record Port
{
	private Port(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static Port FromValue(PortIdentifier value)
	{
		return new Port(value.Value);
	}
}
