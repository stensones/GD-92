using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public interface IManagementTransactionService
{
	Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
		ManagementTransactionRequest request,
		CancellationToken cancellationToken);

	Task<RouterParameterRequestStatus> WaitForCompletionAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		CancellationToken cancellationToken);
}

public sealed record ManagementTransactionRequest(
	CommunicationsAddress Source,
	CommunicationsAddress Destination,
	Func<SequenceNumber, Envelope> CreateEnvelope,
	ManagementTransactionKind Kind,
	CommunicationsAddress? NodeLoginUserAgentAddress = null);

public enum ManagementTransactionKind
{
	ParameterRequest,
	NodeLogin,
	NodeLogoff
}
