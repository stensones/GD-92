using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public sealed class NodeManagerParameterBootstrapper
{
	private readonly INodeManagerParameterStore store;

	public NodeManagerParameterBootstrapper(INodeManagerParameterStore store)
	{
		this.store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public async ValueTask<NodeManagerCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		NodeManagerParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var portNumber = await this.LoadOrBootstrapParameterAsync(
			NodeManagerParameterCatalogue.PortNumber,
			configuration.PortNumber,
			cancellationToken);
		var agentType = await this.LoadOrBootstrapParameterAsync(
			NodeManagerParameterCatalogue.AgentType,
			configuration.AgentType,
			cancellationToken);

		return NodeManagerCurrentParameterProjection.FromNonVolatileParameters(
			portNumber,
			agentType);
	}

	private async ValueTask<ParameterValue> LoadOrBootstrapParameterAsync(
		ParameterNumber parameterNumber,
		ParameterValue bootstrapValue,
		CancellationToken cancellationToken)
	{
		var permanentValue = await this.store.GetAsync(
			ParameterTable.Permanent,
			parameterNumber,
			cancellationToken);
		if (permanentValue is null)
		{
			permanentValue = bootstrapValue;
			await this.store.StoreAsync(
				ParameterTable.Permanent,
				parameterNumber,
				permanentValue,
				cancellationToken);
		}

		var nonVolatileValue = await this.store.GetAsync(
			ParameterTable.NonVolatile,
			parameterNumber,
			cancellationToken);
		if (nonVolatileValue is null)
		{
			nonVolatileValue = permanentValue;
			await this.store.StoreAsync(
				ParameterTable.NonVolatile,
				parameterNumber,
				nonVolatileValue,
				cancellationToken);
		}

		return nonVolatileValue;
	}
}
