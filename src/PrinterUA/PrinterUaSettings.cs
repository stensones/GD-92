using Stensones.GD92.Fields;

namespace PrinterUA;

public sealed record PrinterUaSettings(
	CommunicationsAddress LocalAddress,
	CommunicationsAddress LocalRouter,
	CommunicationsAddress ControlAddress,
	ProtocolVersion ProtocolVersion,
	string? PrinterHostName = null);
