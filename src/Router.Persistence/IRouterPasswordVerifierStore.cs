using Stensones.GD92.Fields;

namespace Router.Persistence;

public interface IRouterPasswordVerifierStore
{
	ValueTask<PasswordVerifier?> GetAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken = default);

	ValueTask StoreAsync(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default);
}
