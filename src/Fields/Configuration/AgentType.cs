namespace Stensones.GD92.Fields;

public sealed record AgentType : IGD9Field
{
	private const int BitCount = 8;

	private AgentType(AgentTypeValue value) => this.Value = value;

	public AgentTypeValue Value { get; }

	public static AgentType FromValue(AgentTypeValue value)
	{
		if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new AgentType(value);
	}

	public static AgentType FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromValue((AgentTypeValue)buffer.ReadUnsignedBits(BitCount));

	public byte[] ToWireValue() => [(byte)this.Value];
}
