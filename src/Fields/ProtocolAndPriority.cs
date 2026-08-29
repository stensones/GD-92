namespace Stensones.GD92.Fields;

public sealed record ProtocolAndPriority : IGD9Field
{
	private ProtocolAndPriority(MessagePriority priority, ProtocolVersion protocolVersion)
	{
		this.Priority = priority;
		this.ProtocolVersion = protocolVersion;
	}

	public MessagePriority Priority { get; }
	public ProtocolVersion ProtocolVersion { get; }

	public static ProtocolAndPriority FromValues(MessagePriority priority, ProtocolVersion protocolVersion)
	{
		ArgumentNullException.ThrowIfNull(priority);
		ArgumentNullException.ThrowIfNull(protocolVersion);

		return new ProtocolAndPriority(priority, protocolVersion);
	}

	public static ProtocolAndPriority FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var value = (byte)buffer.ReadUnsignedBits(8);

		return FromValues(
			MessagePriority.FromValue((byte)(value >> 4)),
			ProtocolVersion.FromValue((byte)(value & 0x0F)));
	}

	public byte[] ToWireValue()
	{
		return [(byte)((this.Priority.Value << 4) | this.ProtocolVersion.Value)];
	}
}
