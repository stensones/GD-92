using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace ParticipantParameters;

public sealed class ParticipantParameterBootstrapper
{
	private readonly IParticipantParameterStore store;

	public ParticipantParameterBootstrapper(IParticipantParameterStore store)
	{
		this.store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public ValueTask<IReadOnlyDictionary<ParameterNumber, ParameterValue>>
		LoadNonVolatileValuesAsync(
			IEnumerable<ParameterBootstrapValue> bootstrapValues,
			CancellationToken cancellationToken = default)
	{
		return this.InitializeAsync(
			bootstrapValues,
			static (nonVolatileValues, _) => ValueTask.FromResult(nonVolatileValues),
			cancellationToken);
	}

	public async ValueTask<T> InitializeAsync<T>(
		IEnumerable<ParameterBootstrapValue> bootstrapValues,
		Func<IReadOnlyDictionary<ParameterNumber, ParameterValue>, CancellationToken, ValueTask<T>>
			createProjection,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(createProjection);

		var valuesByNumber = ValidateBootstrapValues(bootstrapValues);
		return await this.store.ExecuteInitializationAsync(
			async initializeCancellationToken =>
			{
				var nonVolatileValues = await this.LoadNonVolatileValuesCoreAsync(
					valuesByNumber.Values,
					initializeCancellationToken);

				return await createProjection(nonVolatileValues, initializeCancellationToken);
			},
			cancellationToken);
	}

	private static Dictionary<ParameterNumber, ParameterBootstrapValue> ValidateBootstrapValues(
		IEnumerable<ParameterBootstrapValue> bootstrapValues)
	{
		ArgumentNullException.ThrowIfNull(bootstrapValues);

		var valuesByNumber = new Dictionary<ParameterNumber, ParameterBootstrapValue>();
		foreach (var bootstrapValue in bootstrapValues)
		{
			ArgumentNullException.ThrowIfNull(bootstrapValue);

			if (!valuesByNumber.TryAdd(bootstrapValue.ParameterNumber, bootstrapValue))
			{
				throw new ArgumentException(
					$"Bootstrap values contain duplicate Parameter Number {bootstrapValue.ParameterNumber.Value}.",
					nameof(bootstrapValues));
			}
		}

		return valuesByNumber;
	}

	private async ValueTask<IReadOnlyDictionary<ParameterNumber, ParameterValue>>
		LoadNonVolatileValuesCoreAsync(
			IEnumerable<ParameterBootstrapValue> bootstrapValues,
			CancellationToken cancellationToken)
	{
		var nonVolatileValues = new Dictionary<ParameterNumber, ParameterValue>();
		foreach (var bootstrapValue in bootstrapValues)
		{
			var permanentValue = await this.store.GetAsync(
				ParameterTable.Permanent,
				bootstrapValue.ParameterNumber,
				cancellationToken);
			if (permanentValue is null)
			{
				permanentValue = bootstrapValue.InitialValue;
				await this.store.StoreAsync(
					ParameterTable.Permanent,
					bootstrapValue.ParameterNumber,
					permanentValue,
					cancellationToken);
			}

			var nonVolatileValue = await this.store.GetAsync(
				ParameterTable.NonVolatile,
				bootstrapValue.ParameterNumber,
				cancellationToken);
			if (nonVolatileValue is null)
			{
				nonVolatileValue = permanentValue;
				await this.store.StoreAsync(
					ParameterTable.NonVolatile,
					bootstrapValue.ParameterNumber,
					nonVolatileValue,
					cancellationToken);
			}

			nonVolatileValues.Add(bootstrapValue.ParameterNumber, nonVolatileValue);
		}

		return nonVolatileValues;
	}
}
