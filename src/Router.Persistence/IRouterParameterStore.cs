using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace Router.Persistence;

public interface IRouterParameterStore
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
