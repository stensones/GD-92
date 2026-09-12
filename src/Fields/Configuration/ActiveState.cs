namespace Stensones.GD92.Fields;

public sealed record ActiveState : IGD9Field
{
	private const int BitCount = 8;

	private ActiveState(ActiveStateValue value) => this.Value = value;

	public ActiveStateValue Value { get; }

	public static ActiveState FromValue(ActiveStateValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new ActiveState(value);
	}

	public static ActiveState FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((ActiveStateValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
