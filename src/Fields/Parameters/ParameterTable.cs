namespace Stensones.GD92.Fields;

public sealed record ParameterTable : IGD9Field
{
	private ParameterTable(byte value)
	{
		this.Value = value;
	}

	public byte Value { get; }

	public static ParameterTable Permanent { get; } = FromValue(ParameterTableIdentifier.Permanent);
	public static ParameterTable NonVolatile { get; } = FromValue(ParameterTableIdentifier.NonVolatile);
	public static ParameterTable Current { get; } = FromValue(ParameterTableIdentifier.Current);

	public static ParameterTable FromValue(ParameterTableIdentifier value)
	{
		return new ParameterTable(value.Value);
	}

	public static ParameterTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue(ParameterTableIdentifier.FromValue((byte)buffer.ReadUnsignedBits(8)));
	}

	public byte[] ToWireValue()
	{
		return [this.Value];
	}
}
