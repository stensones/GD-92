using AwesomeAssertions;
using ParticipantParameters;
using PrinterUA.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace PrinterUA.Persistence.Tests.Unit;

public sealed class PrinterUaParameterBootstrapperTests
{
	[Fact]
	public async Task Seeds_common_ua_parameters_in_both_persistent_tables_and_projects_non_volatile_values()
	{
		var store = new InMemoryPrinterUaParameterStore();
		var bootstrapper = new PrinterUaParameterBootstrapper(store);
		var printerAddress = CreateAddress(26, 100, 2);
		var nodeManagerAddress = CreateAddress(26, 100, 25);

		var current = await bootstrapper.LoadCurrentParameterProjectionAsync(
			PrinterUaParameterBootstrapConfiguration.FromAddresses(
				printerAddress,
				nodeManagerAddress));

		var expectedValues = new Dictionary<byte, byte[]>
		{
			[1] = [2],
			[2] = [4],
			[3] = [26, 25, 25]
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
	public async Task Seeds_canonical_printer_specific_defaults_in_both_persistent_tables()
	{
		var store = new InMemoryPrinterUaParameterStore();
		var bootstrapper = new PrinterUaParameterBootstrapper(store);

		await bootstrapper.LoadCurrentParameterProjectionAsync(
			PrinterUaParameterBootstrapConfiguration.FromAddresses(
				CreateAddress(26, 100, 2),
				CreateAddress(26, 100, 25)));

		var expectedValues = new Dictionary<byte, byte[]>
		{
			[21] = AddressRange.FromValues(
				CreateAddress(26, 100, 0),
				CreateAddress(26, 100, 63)).ToWireValue(),
			[22] = ProtocolBoolean.True.ToWireValue(),
			[23] = AddressTable.FromEntries().ToWireValue(),
			[24] = ProtocolBoolean.False.ToWireValue()
		};

		foreach (var parameterTable in new[] { ParameterTable.Permanent, ParameterTable.NonVolatile })
		{
			foreach (var (number, expectedValue) in expectedValues)
			{
				(await store.GetAsync(parameterTable, ParameterNumber.FromValue(number)))!
					.ToWireValue().Should().Equal(expectedValue);
			}
		}
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class InMemoryPrinterUaParameterStore : IParticipantParameterStore
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
