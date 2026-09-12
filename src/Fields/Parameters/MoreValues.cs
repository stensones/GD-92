namespace Stensones.GD92.Fields;

public sealed record MoreValues : IGD9Field
{
	private MoreValues(ProtocolBoolean value)
	{
		this.Value = value;
	}

	public ProtocolBoolean Value { get; }

	public static MoreValues No { get; } = FromValue(ProtocolBoolean.False);
	public static MoreValues Yes { get; } = FromValue(ProtocolBoolean.True);

	public static MoreValues FromValue(ProtocolBoolean value)
	{
		return new MoreValues(value);
	}

	public static MoreValues FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ProtocolBoolean.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue() => this.Value.ToWireValue();
}
