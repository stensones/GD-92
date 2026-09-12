namespace Stensones.GD92.Fields;

public sealed record MtaStatus : IGD9Field
{
	private const int BitCount = 8;

	private MtaStatus(MtaStatusValue value) => this.Value = value;

	public MtaStatusValue Value { get; }

	public static MtaStatus FromValue(MtaStatusValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new MtaStatus(value);
	}

	public static MtaStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((MtaStatusValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
