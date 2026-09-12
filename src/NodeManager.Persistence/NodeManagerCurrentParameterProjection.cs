using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class NodeManagerCurrentParameterProjection
{
	private readonly ParameterValue portNumber;
	private readonly ParameterValue agentType;
	private readonly CommunicationsAddress controlAddress;

	private NodeManagerCurrentParameterProjection(
		ParameterValue portNumber,
		ParameterValue agentType,
		CommunicationsAddress controlAddress)
	{
		this.portNumber = portNumber;
		this.agentType = agentType;
		this.controlAddress = controlAddress;
	}

	public ParameterValue Get(ParameterNumber parameterNumber)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return parameterNumber == NodeManagerParameterCatalogue.PortNumber
			? this.portNumber
			: parameterNumber == NodeManagerParameterCatalogue.AgentType
				? this.agentType
				: parameterNumber == NodeManagerParameterCatalogue.ControlAddress
					? NodeManagerParameterCatalogue.EncodeControlAddress(this.controlAddress)
				: throw new ArgumentOutOfRangeException(
					nameof(parameterNumber),
					"NodeManager does not own the requested Parameter.");
	}

	public static NodeManagerCurrentParameterProjection FromNonVolatileParameters(
		ParameterValue portNumber,
		ParameterValue agentType,
		CommunicationsAddress controlAddress)
	{
		ArgumentNullException.ThrowIfNull(portNumber);
		ArgumentNullException.ThrowIfNull(agentType);
		ArgumentNullException.ThrowIfNull(controlAddress);

		return new NodeManagerCurrentParameterProjection(portNumber, agentType, controlAddress);
	}
}
