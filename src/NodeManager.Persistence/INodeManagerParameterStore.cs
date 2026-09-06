using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public interface INodeManagerParameterStore
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
}
