using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Parameters;

public sealed class RouterParameterRequestService(
	RouterParameterRequestSettings settings,
	IParticipantParameterRequestService participantParameterRequests,
	IManagementTransactionService managementTransactions) : IRouterParameterRequestService
{
	public RouterParameterRequestService(
		RouterParameterRequestSettings settings,
		IManagementTransactionService managementTransactions) :
		this(
			settings,
			new ParticipantParameterRequestService(settings, managementTransactions),
			managementTransactions)
	{
	}

	public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken) =>
		this.RequestLocalRouterCurrentParameter(ParameterNumber.FromValue(1), cancellationToken);

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterCurrentParameter(
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken)
	{
		return await this.RequestLocalRouterParameter(
			ParameterTable.Current,
			parameterNumber,
			cancellationToken);
	}

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterParameter(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return await participantParameterRequests.RequestParameter(
			settings.LocalRouter,
			parameterTable,
			parameterNumber,
			cancellationToken);
	}

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterParameterEntries(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterEntrySelection entrySelection,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);
		ArgumentNullException.ThrowIfNull(entrySelection);

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
					ParameterRequestMultiple.FromFields(
						parameterTable,
						parameterNumber,
						entrySelection)),
				ManagementTransactionKind.ParameterRequest),
			cancellationToken);
	}

	public async Task<RouterParameterRequestStatusIdentifier> ModifyLocalRouterParameter(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);
		ArgumentNullException.ThrowIfNull(parameterValue);

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
						parameterTable,
						parameterNumber,
						parameterValue)),
				ManagementTransactionKind.ParameterModification),
			cancellationToken);
	}

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
		CommunicationsAddress communicationsAddress,
		PasswordLevel passwordLevel,
		PasswordValue password,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(communicationsAddress);
		ArgumentNullException.ThrowIfNull(passwordLevel);

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
								passwordLevel,
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
