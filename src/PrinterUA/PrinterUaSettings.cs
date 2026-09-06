using Stensones.GD92.Fields;

namespace PrinterUA;

public sealed record PrinterUaSettings(
	CommunicationsAddress LocalAddress,
	CommunicationsAddress LocalRouter,
	ProtocolVersion ProtocolVersion);
