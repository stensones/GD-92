namespace Stensones.GD92.Fields;

public sealed record ProtocolVersion
{
	private ProtocolVersion(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static ProtocolVersion FromValue(byte value)
	{
		if (value is < 1 or > 15)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ProtocolVersion(value);
	}
}
