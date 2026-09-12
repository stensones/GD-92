using AwesomeAssertions;
using NodeManager.Persistence;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence.Tests.Unit;

public sealed class NodeManagerParameterBootstrapperTests
{
	[Fact]
	public async Task Seeds_common_ua_parameters_in_both_persistent_tables_and_projects_non_volatile_values()
	{
		var store = new InMemoryNodeManagerParameterStore();
		var bootstrapper = new NodeManagerParameterBootstrapper(store);
		var nodeManagerAddress = CreateAddress(26, 100, 25);

		var current = await bootstrapper.LoadCurrentParameterProjectionAsync(
			NodeManagerParameterBootstrapConfiguration.FromAddress(nodeManagerAddress));

		var expectedValues = new Dictionary<byte, byte[]>
		{
			[1] = [25],
			[2] = [12],
			[3] = nodeManagerAddress.ToWireValue()
		};

		foreach (var (number, expectedValue) in expectedValues)
		{
			var parameterNumber = ParameterNumber.FromValue(number);
			current.Get(parameterNumber).ToWireValue().Should().Equal(expectedValue);
			(await store.GetAsync(ParameterTable.Permanent, parameterNumber))!
				.ToWireValue().Should().Equal(expectedValue);
			(await store.GetAsync(ParameterTable.NonVolatile, parameterNumber))!
				.ToWireValue().Should().Equal(expectedValue);
		}
	}

	[Fact]
	public async Task Retains_bootstrapped_identity_values_when_later_configuration_changes()
	{
		var store = new InMemoryNodeManagerParameterStore();
		var bootstrapper = new NodeManagerParameterBootstrapper(store);
		await bootstrapper.LoadCurrentParameterProjectionAsync(
			NodeManagerParameterBootstrapConfiguration.FromAddress(CreateAddress(26, 100, 25)));

		var current = await bootstrapper.LoadCurrentParameterProjectionAsync(
			NodeManagerParameterBootstrapConfiguration.FromAddress(CreateAddress(26, 100, 24)));

		current.Get(ParameterNumber.FromValue(1)).ToWireValue().Should().Equal([25]);
		current.Get(ParameterNumber.FromValue(2)).ToWireValue().Should().Equal([12]);
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class InMemoryNodeManagerParameterStore :
		IParticipantParameterStore
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

		public ValueTask<T> ExecuteInitializationAsync<T>(
			Func<CancellationToken, ValueTask<T>> initialize,
			CancellationToken cancellationToken = default)
		{
			return initialize(cancellationToken);
		}
	}
}
