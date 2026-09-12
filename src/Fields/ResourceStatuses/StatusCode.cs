namespace Stensones.GD92.Fields;

public sealed record StatusCode : IGD9Field
{
	private const int BitCount = 8;

	private StatusCode(StatusCodeValue value) => this.Value = value;

	public StatusCodeValue Value { get; }

	public static StatusCode FromValue(StatusCodeValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new StatusCode(value);
	}

	public static StatusCode FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((StatusCodeValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
