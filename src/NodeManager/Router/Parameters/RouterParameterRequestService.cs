using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class RouterParameterRequestService(
	RouterParameterRequestSettings settings,
	IPendingDeliveryRegistry pendingDeliveries,
	IRouterIngress routerIngress) : IRouterParameterRequestService
{
	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
		CancellationToken cancellationToken)
	{
		var statusIdentifier = pendingDeliveries.Reserve(
			settings.MessageOriginator,
			settings.LocalRouter);
		var envelope = Envelope.FromValues(
			settings.MessageOriginator,
			Destinations.FromAddresses(settings.LocalRouter),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				statusIdentifier.USWR.SequenceNumber,
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(1)));

		await routerIngress.SubmitAsync(envelope, cancellationToken);
		return statusIdentifier;
	}

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
		CommunicationsAddress communicationsAddress,
		PasswordValue password,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(communicationsAddress);

		var statusIdentifier = pendingDeliveries.ReserveNodeLogin(
			settings.MessageOriginator,
			settings.LocalRouter,
			communicationsAddress);
		var envelope = Envelope.FromValues(
			settings.MessageOriginator,
			Destinations.FromAddresses(settings.LocalRouter),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				statusIdentifier.USWR.SequenceNumber,
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue(
					PasswordParameter.FromFields(
						PasswordLevel.FromValue(PasswordLevelNumber.Level1),
						Password.FromValue(password),
						communicationsAddress).ToWireValue())));

		await routerIngress.SubmitAsync(envelope, cancellationToken);
		return statusIdentifier;
	}

	public async Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogoff(
		CancellationToken cancellationToken)
	{
		var statusIdentifier = pendingDeliveries.ReserveNodeLogoff(
			settings.MessageOriginator,
			settings.LocalRouter);
		var envelope = Envelope.FromValues(
			settings.MessageOriginator,
			Destinations.FromAddresses(settings.LocalRouter),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				statusIdentifier.USWR.SequenceNumber,
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue(
					PasswordParameter.FromFields(
						PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
						Password.FromValue(
							PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
						settings.MessageOriginator).ToWireValue())));

		await routerIngress.SubmitAsync(envelope, cancellationToken);
		return statusIdentifier;
	}
}
