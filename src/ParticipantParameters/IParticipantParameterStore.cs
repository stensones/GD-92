using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace ParticipantParameters;

public interface IParticipantParameterStore
{
	ValueTask<ParameterValue?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken = default);

	ValueTask StoreAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue,
		CancellationToken cancellationToken = default);

	ValueTask<T> ExecuteInitializationAsync<T>(
		Func<CancellationToken, ValueTask<T>> initialize,
		CancellationToken cancellationToken = default);
}
