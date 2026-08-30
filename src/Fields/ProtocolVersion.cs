namespace Stensones.GD92.Fields;

public sealed record ProtocolVersion
{
	private ProtocolVersion(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static ProtocolVersion FromValue(ProtocolVersionNumber value)
	{
		return new ProtocolVersion(value.Value);
	}
}
