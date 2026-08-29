namespace Stensones.GD92.Fields;

public sealed record CommunicationsAddress : IGD9Field
{
	private CommunicationsAddress(Brigade brigade, Node node, Port port)
	{
		this.Brigade = brigade;
		this.Node = node;
		this.Port = port;
	}

	public Brigade Brigade { get; }
	public Node Node { get; }
	public Port Port { get; }

	public static CommunicationsAddress FromValues(Brigade brigade, Node node, Port port)
	{
		return new CommunicationsAddress(brigade, node, port);
	}

	public static CommunicationsAddress FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromValues(
			Brigade.FromValue((byte)buffer.ReadUnsignedBits(8)),
			Node.FromValue((ushort)buffer.ReadUnsignedBits(10)),
			Port.FromValue((byte)buffer.ReadUnsignedBits(6)));
	}

	public byte[] ToWireValue()
	{
		return [this.Brigade.Value, (byte)(this.Node.Value >> 2), (byte)((this.Node.Value << 6) | this.Port.Value)];
	}
}
