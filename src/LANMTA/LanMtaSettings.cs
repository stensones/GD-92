using Stensones.GD92.Fields;

namespace LANMTA;

public sealed record LanMtaSettings(
	CommunicationsAddress LocalAddress,
	CommunicationsAddress LocalRouter,
	ProtocolVersion ProtocolVersion);
