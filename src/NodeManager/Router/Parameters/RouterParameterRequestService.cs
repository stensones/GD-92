using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed class RouterParameterRequestService(
	RouterParameterRequestSettings settings,
	IManagementTransactionService managementTransactions) : IRouterParameterRequestService
{
	public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken) =>
		this.RequestLocalRouterCurrentParameter(ParameterNumber.FromValue(1), cancellationToken);

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterCurrentParameter(
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return await managementTransactions.SubmitAsync(
			new ManagementTransactionRequest(
				settings.MessageOriginator,
				settings.LocalRouter,
				sequenceNumber => Envelope.FromValues(
					settings.MessageOriginator,
					Destinations.FromAddresses(settings.LocalRouter),
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

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
		CommunicationsAddress communicationsAddress,
		PasswordValue password,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(communicationsAddress);

		return await managementTransactions.SubmitAsync(
			new ManagementTransactionRequest(
				settings.MessageOriginator,
				settings.LocalRouter,
				sequenceNumber => Envelope.FromValues(
					settings.MessageOriginator,
					Destinations.FromAddresses(settings.LocalRouter),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						sequenceNumber,
						AcknowledgementRequest.Requested),
					SetParameter.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(4),
						ParameterValue.FromWireValue(
							PasswordParameter.FromFields(
								PasswordLevel.FromValue(PasswordLevelNumber.Level1),
								Password.FromValue(password),
								communicationsAddress).ToWireValue()))),
				ManagementTransactionKind.NodeLogin,
				communicationsAddress),
			cancellationToken);
	}

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogoff(
		CancellationToken cancellationToken)
	{
		return await managementTransactions.SubmitAsync(
			new ManagementTransactionRequest(
				settings.MessageOriginator,
				settings.LocalRouter,
				sequenceNumber => Envelope.FromValues(
					settings.MessageOriginator,
					Destinations.FromAddresses(settings.LocalRouter),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						sequenceNumber,
						AcknowledgementRequest.Requested),
					SetParameter.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(4),
						ParameterValue.FromWireValue(
							PasswordParameter.FromFields(
								PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
								Password.FromValue(
									PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
								settings.MessageOriginator).ToWireValue()))),
				ManagementTransactionKind.NodeLogoff),
			cancellationToken);
	}
}
