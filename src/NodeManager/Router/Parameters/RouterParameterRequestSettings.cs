using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record RouterParameterRequestSettings(
	CommunicationsAddress MessageOriginator,
	CommunicationsAddress LocalRouter);
