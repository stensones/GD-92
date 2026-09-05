using Stensones.GD92.Fields;

namespace Router.Persistence;

public interface IRouterLevel1PasswordVerifierStore
{
	ValueTask<PasswordVerifier?> GetAsync(
		ParameterTable parameterTable,
		CancellationToken cancellationToken = default);

	ValueTask StoreAsync(
		ParameterTable parameterTable,
		PasswordVerifier passwordVerifier,
		CancellationToken cancellationToken = default);
}
