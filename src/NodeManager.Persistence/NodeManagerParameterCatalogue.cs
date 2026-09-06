using Stensones.GD92.Fields;

namespace NodeManager.Persistence;

public static class NodeManagerParameterCatalogue
{
	public static ParameterNumber PortNumber { get; } = ParameterNumber.FromValue(1);
	public static ParameterNumber AgentType { get; } = ParameterNumber.FromValue(2);
}
