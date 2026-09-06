using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class NodeManagerParameterBootstrapConfiguration
{
	private NodeManagerParameterBootstrapConfiguration(
		ParameterValue portNumber,
		ParameterValue agentType)
	{
		this.PortNumber = portNumber;
		this.AgentType = agentType;
	}

	public ParameterValue PortNumber { get; }
	public ParameterValue AgentType { get; }

	public static NodeManagerParameterBootstrapConfiguration FromAddress(
		CommunicationsAddress address)
	{
		ArgumentNullException.ThrowIfNull(address);

		return new NodeManagerParameterBootstrapConfiguration(
			ParameterValue.FromWireValue([address.Port.Value]),
			ParameterValue.FromWireValue([12]));
	}
}
