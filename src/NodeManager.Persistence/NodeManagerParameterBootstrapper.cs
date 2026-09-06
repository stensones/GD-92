using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class NodeManagerParameterBootstrapper
{
	private readonly ParticipantParameterBootstrapper parameterBootstrapper;

	public NodeManagerParameterBootstrapper(IParticipantParameterStore store)
	{
		this.parameterBootstrapper = new ParticipantParameterBootstrapper(store);
	}

	public async ValueTask<NodeManagerCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		NodeManagerParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var nonVolatileValues = await this.parameterBootstrapper.LoadNonVolatileValuesAsync(
		[
			ParameterBootstrapValue.FromValues(
				NodeManagerParameterCatalogue.PortNumber,
				configuration.PortNumber),
			ParameterBootstrapValue.FromValues(
				NodeManagerParameterCatalogue.AgentType,
				configuration.AgentType)
		],
		cancellationToken);

		return NodeManagerCurrentParameterProjection.FromNonVolatileParameters(
			nonVolatileValues[NodeManagerParameterCatalogue.PortNumber],
			nonVolatileValues[NodeManagerParameterCatalogue.AgentType]);
	}
}
