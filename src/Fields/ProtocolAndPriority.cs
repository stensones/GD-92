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

	public byte[] ToWireValue()
	{
		return [(byte)((this.Priority.Value << 4) | this.ProtocolVersion.Value)];
	}
}
