using AwesomeAssertions;
using LANMTA.Persistence;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace LANMTA.Persistence.Tests.Unit;

public sealed class LanMtaParameterBootstrapperTests
{
	[Fact]
	public async Task Seeds_permanent_and_non_volatile_identity_parameters_and_projects_the_non_volatile_values()
	{
		var store = new InMemoryLanMtaParameterStore();
		var bootstrapper = new LanMtaParameterBootstrapper(store);

		var current = await bootstrapper.LoadCurrentParameterProjectionAsync(
			LanMtaParameterBootstrapConfiguration.FromAddress(CreateAddress(26, 100, 1)));

		current.Get(ParameterNumber.FromValue(1)).ToWireValue().Should().Equal([1]);
		current.Get(ParameterNumber.FromValue(2)).ToWireValue().Should().Equal([10]);
		current.Get(ParameterNumber.FromValue(3)).ToWireValue().Should().Equal([0]);
		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([1]);
		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(2)))!
			.ToWireValue().Should().Equal([10]);
		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([1]);
		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(2)))!
			.ToWireValue().Should().Equal([10]);
	}

	[Fact]
	public async Task Seeds_the_complete_approved_lan_mta_default_catalogue_in_both_persistent_tables()
	{
		var store = new InMemoryLanMtaParameterStore();
		var bootstrapper = new LanMtaParameterBootstrapper(store);

		await bootstrapper.LoadCurrentParameterProjectionAsync(
			LanMtaParameterBootstrapConfiguration.FromAddress(CreateAddress(26, 100, 1)));

		var expectedValues = new Dictionary<byte, byte[]>
		{
			[1] = Word8.FromValue(1).ToWireValue(),
			[2] = AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent).ToWireValue(),
			[3] = MtaStatus.FromValue(MtaStatusValue.Idle).ToWireValue(),
			[4] = ProtocolBoolean.True.ToWireValue(),
			[5] = FrameTransmitCount.FromValue(0).ToWireValue(),
			[6] = FrameReceiveCount.FromValue(0).ToWireValue(),
			[7] = FrameTransmitFailureCount.FromValue(0).ToWireValue(),
			[8] = FrameReceiveFailureCount.FromValue(0).ToWireValue(),
			[9] = MtaMinimumMessagePriority
				.FromValue(MessagePriorityLevel.FromValue(3))
				.ToWireValue(),
			[10] = DestinationNodes.FromAddressRanges().ToWireValue(),
			[21] = LanAddress.FromValue(SevenBitAsciiString.FromValue("station-end-lan")).ToWireValue()
		};

		foreach (var (number, expectedValue) in expectedValues)
		{
			(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(number)))!
				.ToWireValue().Should().Equal(expectedValue);
			(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(number)))!
				.ToWireValue().Should().Equal(expectedValue);
		}
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class InMemoryLanMtaParameterStore : IParticipantParameterStore
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
