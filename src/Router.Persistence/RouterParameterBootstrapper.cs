using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public sealed class RouterParameterBootstrapper
{
	private static readonly ParameterNumber BrigadeOrAgencyNumber =
		ParameterNumber.FromValue(1);

	private readonly IRouterParameterStore store;

	public RouterParameterBootstrapper(IRouterParameterStore store)
	{
		this.store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public async ValueTask<RouterCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		BrigadeOrAgencyIdentifier configuredBrigadeOrAgencyIdentifier,
		CancellationToken cancellationToken = default)
	{
		var permanentValue = await this.store.GetAsync(
			ParameterTable.Permanent,
			BrigadeOrAgencyNumber,
			cancellationToken);

		if (permanentValue is null)
		{
			permanentValue = ParameterValue.FromWireValue(
				configuredBrigadeOrAgencyIdentifier.ToWireValue());
			await this.store.StoreAsync(
				ParameterTable.Permanent,
				BrigadeOrAgencyNumber,
				permanentValue,
				cancellationToken);
		}

		var nonVolatileValue = await this.store.GetAsync(
			ParameterTable.NonVolatile,
			BrigadeOrAgencyNumber,
			cancellationToken);

		if (nonVolatileValue is null)
		{
			nonVolatileValue = permanentValue;
			await this.store.StoreAsync(
				ParameterTable.NonVolatile,
				BrigadeOrAgencyNumber,
				nonVolatileValue,
				cancellationToken);
		}

		return RouterCurrentParameterProjection.FromParameterOneValue(nonVolatileValue);
	}
}
