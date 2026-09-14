using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public interface IRouterParameterRequestService
{
	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterCurrentParameter(
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterParameter(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterParameterEntries(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterEntrySelection entrySelection,
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> ModifyLocalRouterParameter(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue,
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
		CommunicationsAddress communicationsAddress,
		PasswordValue password,
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogoff(
		CancellationToken cancellationToken);
}
