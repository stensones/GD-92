namespace Stensones.GD92.Fields;

public sealed record ParameterEntryIndex : IGD9Field
{
	private ParameterEntryIndex(ushort value)
	{
		this.Value = value;
	}

	public ushort Value { get; }

	public static ParameterEntryIndex FromValue(ushort value)
	{
		return new ParameterEntryIndex(value);
	}

	public static ParameterEntryIndex FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((ushort)buffer.ReadUnsignedBits(16));
	}

	public byte[] ToWireValue()
	{
		return [(byte)(this.Value >> 8), (byte)this.Value];
	}
}
