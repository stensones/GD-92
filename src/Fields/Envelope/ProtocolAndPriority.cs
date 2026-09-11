namespace Stensones.GD92.Fields;

public sealed record ProtocolAndPriority : IGD9Field
{
	private const int PackedFieldBitCount = 8;
	private const int ProtocolVersionBitCount = 4;
	private const byte ProtocolVersionMask = 0x0F;

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
		var value = (byte)buffer.ReadUnsignedBits(PackedFieldBitCount);

		return FromValues(
			MessagePriority.FromValue(MessagePriorityLevel.FromValue((byte)(value >> ProtocolVersionBitCount))),
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue((byte)(value & ProtocolVersionMask))));
	}

	public byte[] ToWireValue()
	{
		return [(byte)((this.Priority.Value << ProtocolVersionBitCount) | this.ProtocolVersion.Value)];
	}
}
