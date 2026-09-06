using AwesomeAssertions;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace ParticipantParameters.Tests.Unit;

public sealed class ParticipantParameterBootstrapperTests
{
	[Fact]
	public async Task Seeds_permanent_and_non_volatile_values_when_the_Participant_Parameter_Store_is_empty()
	{
		var store = new InMemoryParticipantParameterStore();
		var bootstrapper = new ParticipantParameterBootstrapper(store);

		var nonVolatileValues = await bootstrapper.LoadNonVolatileValuesAsync(
		[
			ParameterBootstrapValue.FromValues(
				ParameterNumber.FromValue(1),
				ParameterValue.FromWireValue([25])),
			ParameterBootstrapValue.FromValues(
				ParameterNumber.FromValue(2),
				ParameterValue.FromWireValue([12]))
		]);

		nonVolatileValues[ParameterNumber.FromValue(1)].ToWireValue().Should().Equal([25]);
		nonVolatileValues[ParameterNumber.FromValue(2)].ToWireValue().Should().Equal([12]);
		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([25]);
		(await store.GetAsync(ParameterTable.Permanent, ParameterNumber.FromValue(2)))!
			.ToWireValue().Should().Equal([12]);
		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(1)))!
			.ToWireValue().Should().Equal([25]);
		(await store.GetAsync(ParameterTable.NonVolatile, ParameterNumber.FromValue(2)))!
			.ToWireValue().Should().Equal([12]);
	}

	[Fact]
	public async Task Retains_existing_non_volatile_values_over_permanent_and_bootstrap_values()
	{
		var store = new InMemoryParticipantParameterStore();
		var parameterNumber = ParameterNumber.FromValue(1);
		await store.StoreAsync(
			ParameterTable.Permanent,
			parameterNumber,
			ParameterValue.FromWireValue([25]));
		await store.StoreAsync(
			ParameterTable.NonVolatile,
			parameterNumber,
			ParameterValue.FromWireValue([24]));
		var bootstrapper = new ParticipantParameterBootstrapper(store);

		var nonVolatileValues = await bootstrapper.LoadNonVolatileValuesAsync(
		[
			ParameterBootstrapValue.FromValues(
				parameterNumber,
				ParameterValue.FromWireValue([23]))
		]);

		nonVolatileValues[parameterNumber].ToWireValue().Should().Equal([24]);
		(await store.GetAsync(ParameterTable.Permanent, parameterNumber))!
			.ToWireValue().Should().Equal([25]);
		(await store.GetAsync(ParameterTable.NonVolatile, parameterNumber))!
			.ToWireValue().Should().Equal([24]);
	}

	[Fact]
	public async Task Rejects_duplicate_Parameter_Numbers_before_accessing_the_Participant_Parameter_Store()
	{
		var store = new TrackingParticipantParameterStore();
		var bootstrapper = new ParticipantParameterBootstrapper(store);

		var load = async () => await bootstrapper.LoadNonVolatileValuesAsync(
		[
			ParameterBootstrapValue.FromValues(
				ParameterNumber.FromValue(1),
				ParameterValue.FromWireValue([25])),
			ParameterBootstrapValue.FromValues(
				ParameterNumber.FromValue(1),
				ParameterValue.FromWireValue([24]))
		]);

		await load.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*duplicate Parameter Number 1*");
		store.OperationCount.Should().Be(0);
	}

	[Fact]
	public async Task Propagates_Participant_Parameter_Store_failures()
	{
		var bootstrapper = new ParticipantParameterBootstrapper(
			new FailingParticipantParameterStore());

		var load = async () => await bootstrapper.LoadNonVolatileValuesAsync(
		[
			ParameterBootstrapValue.FromValues(
				ParameterNumber.FromValue(1),
				ParameterValue.FromWireValue([25]))
		]);

		await load.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Participant Parameter Store is unavailable.");
	}

	private sealed class InMemoryParticipantParameterStore : IParticipantParameterStore
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

	private sealed class FailingParticipantParameterStore : IParticipantParameterStore
	{
		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			throw new InvalidOperationException("Participant Parameter Store is unavailable.");
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default)
		{
			throw new InvalidOperationException("Participant Parameter Store is unavailable.");
		}
	}

	private sealed class TrackingParticipantParameterStore : IParticipantParameterStore
	{
		public int OperationCount { get; private set; }

		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			this.OperationCount++;

			return ValueTask.FromResult<ParameterValue?>(null);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default)
		{
			this.OperationCount++;

			return ValueTask.CompletedTask;
		}
	}
}
