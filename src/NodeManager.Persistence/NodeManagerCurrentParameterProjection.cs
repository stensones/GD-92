using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class NodeManagerCurrentParameterProjection
{
	private readonly ParameterValue portNumber;
	private readonly ParameterValue agentType;

	private NodeManagerCurrentParameterProjection(
		ParameterValue portNumber,
		ParameterValue agentType)
	{
		this.portNumber = portNumber;
		this.agentType = agentType;
	}

	public ParameterValue Get(ParameterNumber parameterNumber)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return parameterNumber == NodeManagerParameterCatalogue.PortNumber
			? this.portNumber
			: parameterNumber == NodeManagerParameterCatalogue.AgentType
				? this.agentType
				: throw new ArgumentOutOfRangeException(
					nameof(parameterNumber),
					"NodeManager does not own the requested Parameter.");
	}

	public static NodeManagerCurrentParameterProjection FromNonVolatileParameters(
		ParameterValue portNumber,
		ParameterValue agentType)
	{
		ArgumentNullException.ThrowIfNull(portNumber);
		ArgumentNullException.ThrowIfNull(agentType);

		return new NodeManagerCurrentParameterProjection(portNumber, agentType);
	}
}
