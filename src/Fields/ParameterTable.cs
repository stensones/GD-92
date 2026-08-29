namespace Stensones.GD92.Fields;

public sealed record ParameterTable : Word8
{
	private ParameterTable(byte value)
		: base(value)
	{
	}

	public static ParameterTable Permanent { get; } = FromValue(0);
	public static ParameterTable NonVolatile { get; } = FromValue(1);
	public static ParameterTable Current { get; } = FromValue(2);

	public new static ParameterTable FromValue(byte value)
	{
		if (value > 2)
		{
			throw new ArgumentOutOfRangeException(nameof(value));
		}

		return new ParameterTable(value);
	}

	public new static ParameterTable FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValue((byte)buffer.ReadUnsignedBits(8));
	}
}
