namespace Stensones.GD92.Fields;

public sealed record IncidentNumber : IGD9Field
{
	private IncidentNumber(uint value)
	{
		this.Value = value;
	}

	public uint Value { get; }

	public static IncidentNumber FromValue(uint value)
	{
		return new IncidentNumber(value);
	}

	public static IncidentNumber FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(buffer.ReadUnsignedBits(32));
	}

	public byte[] ToWireValue()
	{
		return [
			(byte)(this.Value >> 24),
			(byte)(this.Value >> 16),
			(byte)(this.Value >> 8),
			(byte)this.Value
		];
	}
}
