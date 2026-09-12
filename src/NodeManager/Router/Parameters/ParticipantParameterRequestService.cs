using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed class ParticipantParameterRequestService(
	RouterParameterRequestSettings settings,
	IManagementTransactionService managementTransactions) : IParticipantParameterRequestService
{
	public async Task<RouterParameterRequestStatusIdentifier> RequestCurrentParameter(
		CommunicationsAddress destination,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(destination);
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return await managementTransactions.SubmitAsync(
			new ManagementTransactionRequest(
				settings.MessageOriginator,
				destination,
				sequenceNumber => Envelope.FromValues(
					settings.MessageOriginator,
					Destinations.FromAddresses(destination),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						sequenceNumber,
						AcknowledgementRequest.Requested),
					ParameterRequest.FromFields(
						ParameterTable.Current,
						parameterNumber)),
				ManagementTransactionKind.ParameterRequest),
			cancellationToken);
	}
}
