namespace Stensones.GD92.Fields;

public sealed record CommunicationsAddress : IGD9Field
{
	private const int BrigadeBitCount = 8;
	private const int NodeBitCount = 10;
	private const int PortBitCount = 6;
	private const int NodeBitsInFinalOctet = 2;

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
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue((byte)buffer.ReadUnsignedBits(BrigadeBitCount))),
			Node.FromValue(NodeIdentifier.FromValue((ushort)buffer.ReadUnsignedBits(NodeBitCount))),
			Port.FromValue(PortIdentifier.FromValue((byte)buffer.ReadUnsignedBits(PortBitCount))));
	}

	public byte[] ToWireValue()
	{
		return [
			this.Brigade.Value.EncodedOctet,
			(byte)(this.Node.Value >> NodeBitsInFinalOctet),
			(byte)((this.Node.Value << PortBitCount) | this.Port.Value)
		];
	}
}
