using AwesomeAssertions;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence.Tests.Unit;

public sealed class RouterParameterBootstrapperTests
{
	[Fact]
	public async Task Seeds_permanent_and_non_volatile_router_parameter_one_when_the_store_is_empty()
	{
		var store = new InMemoryRouterParameterStore();
		var bootstrapper = new RouterParameterBootstrapper(store);

		var currentParameters = await bootstrapper.LoadCurrentParameterProjectionAsync(
			BrigadeOrAgencyIdentifier.FromValue(26));
		var permanentValue = await store.GetAsync(
			ParameterTable.Permanent,
			ParameterNumber.FromValue(1));
		var nonVolatileValue = await store.GetAsync(
			ParameterTable.NonVolatile,
			ParameterNumber.FromValue(1));

		currentParameters.BrigadeOrAgencyIdentifier.ToWireValue().Should().Equal([26]);
		permanentValue.Should().NotBeNull();
		permanentValue!.ToWireValue().Should().Equal([26]);
		nonVolatileValue.Should().NotBeNull();
		nonVolatileValue!.ToWireValue().Should().Equal([26]);
	}

	private sealed class InMemoryRouterParameterStore : IRouterParameterStore
	{
		private readonly Dictionary<(ParameterTable Table, ParameterNumber Number), ParameterValue> values = [];

		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			this.values.TryGetValue((parameterTable, parameterNumber), out var value);

			return ValueTask.FromResult(value);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default)
		{
			this.values[(parameterTable, parameterNumber)] = parameterValue;

			return ValueTask.CompletedTask;
		}
	}
}
