using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace Router;

public sealed class RouterIngressReceiver : IRouterIngressReceiver
{
	private readonly RouterParameterRequestHandler parameterRequestHandler;
	private readonly IUserAgentIngress userAgentIngress;
	private readonly ILocalParticipantIngress localParticipantIngress;
	private readonly ILogger<RouterIngressReceiver> logger;

	public RouterIngressReceiver(
		RouterParameterRequestHandler parameterRequestHandler,
		IUserAgentIngress userAgentIngress,
		ILocalParticipantIngress localParticipantIngress,
		ILogger<RouterIngressReceiver> logger)
	{
		this.parameterRequestHandler = parameterRequestHandler ??
			throw new ArgumentNullException(nameof(parameterRequestHandler));
		this.userAgentIngress = userAgentIngress ??
			throw new ArgumentNullException(nameof(userAgentIngress));
		this.localParticipantIngress = localParticipantIngress ??
			throw new ArgumentNullException(nameof(localParticipantIngress));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		this.logger.LogInformation(
			"Router received Message Type {MessageType} from {Source} for {DestinationCount} destination(s).",
			envelope.Contents.Type.Value,
			envelope.Source,
			envelope.Destinations.Addresses.Count);

		var handling = await this.parameterRequestHandler.HandleAsync(
			envelope,
			cancellationToken);
		var response = handling.Response;

		if (response is not null)
		{
			await this.userAgentIngress.DeliverAsync(response, cancellationToken);
			this.logger.LogInformation(
				"Router returned Message Type {MessageType} to {Destination}.",
				response.Contents.Type.Value,
				response.Destinations.Addresses[0]);
		}
		else if (this.IsForLocalNonRouterDestination(envelope))
		{
			if (IsParticipantManagementRequest(envelope))
			{
				await this.localParticipantIngress.DeliverAsync(envelope, cancellationToken);
			}
			else
			{
				await this.userAgentIngress.DeliverAsync(envelope, cancellationToken);
			}
		}
		else
		{
			this.logger.LogWarning(
				"Router did not handle Message Type {MessageType}: {HandlingStatus}.",
				envelope.Contents.Type.Value,
				handling.Status);
		}
	}

	private static bool IsParticipantManagementRequest(Envelope envelope)
	{
		return envelope.Contents is ParameterRequest or ParameterRequestMultiple or SetParameter;
	}

	private bool IsForLocalNonRouterDestination(Envelope envelope)
	{
		if (envelope.Destinations.Addresses.Count != 1)
		{
			return false;
		}

		var destination = envelope.Destinations.Addresses[0];
		var localAddress = this.parameterRequestHandler.LocalAddress;

		return destination.Brigade == localAddress.Brigade &&
			destination.Node == localAddress.Node &&
			destination.Port.Value != 0;
	}
}
