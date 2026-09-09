using Microsoft.Extensions.Logging;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Router;

internal sealed class RouterLocalDelivery
{
	private readonly CommunicationsAddress localRouter;
	private readonly RouterParameterRead routerParameterRead;
	private readonly NodeLogin nodeLogin;
	private readonly Level1PasswordModification level1PasswordModification;
	private readonly IUserAgentIngress userAgentIngress;
	private readonly ILocalParticipantIngress localParticipantIngress;
	private readonly ILogger<RouterLocalDelivery> logger;

	public RouterLocalDelivery(
		CommunicationsAddress localRouter,
		RouterParameterRead routerParameterRead,
		NodeLogin nodeLogin,
		Level1PasswordModification level1PasswordModification,
		IUserAgentIngress userAgentIngress,
		ILocalParticipantIngress localParticipantIngress,
		ILogger<RouterLocalDelivery> logger)
	{
		this.localRouter = localRouter ?? throw new ArgumentNullException(nameof(localRouter));
		this.routerParameterRead = routerParameterRead ??
			throw new ArgumentNullException(nameof(routerParameterRead));
		this.nodeLogin = nodeLogin ?? throw new ArgumentNullException(nameof(nodeLogin));
		this.level1PasswordModification = level1PasswordModification ??
			throw new ArgumentNullException(nameof(level1PasswordModification));
		this.userAgentIngress = userAgentIngress ??
			throw new ArgumentNullException(nameof(userAgentIngress));
		this.localParticipantIngress = localParticipantIngress ??
			throw new ArgumentNullException(nameof(localParticipantIngress));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Destinations.Addresses.Count != 1 ||
			envelope.Destinations.Addresses[0].Brigade != this.localRouter.Brigade ||
			envelope.Destinations.Addresses[0].Node != this.localRouter.Node)
		{
			this.logger.LogWarning(
				"Router did not locally deliver Message Type {MessageType}.",
				envelope.Contents.Type.Value);
			return;
		}

		var destination = envelope.Destinations.Addresses[0];
		if (destination == this.localRouter)
		{
			var response = await this.HandleRouterEnvelopeAsync(envelope, cancellationToken);
			if (response is not null)
			{
				await this.userAgentIngress.DeliverAsync(response, cancellationToken);
				this.logger.LogInformation(
					"Router returned Message Type {MessageType} to {Destination}.",
					response.Contents.Type.Value,
					response.Destinations.Addresses[0]);
			}
			else
			{
				this.logger.LogWarning(
					"Router did not handle Message Type {MessageType}.",
					envelope.Contents.Type.Value);
			}

			return;
		}

		if (destination.Port.Value != 0 &&
			envelope.Contents is ParameterRequest or ParameterRequestMultiple or SetParameter)
		{
			await this.localParticipantIngress.DeliverAsync(envelope, cancellationToken);
			return;
		}

		if (destination.Port.Value != 0)
		{
			await this.userAgentIngress.DeliverAsync(envelope, cancellationToken);
		}
	}

	private ValueTask<Envelope?> HandleRouterEnvelopeAsync(
		Envelope envelope,
		CancellationToken cancellationToken)
	{
		return envelope.Contents switch
		{
			ParameterRequest => this.routerParameterRead.HandleAsync(envelope, cancellationToken),
			SetParameter
			{
				ParameterTable: var table,
				ParameterNumber: var number
			} when table == ParameterTable.Current &&
				number == RouterParameterCatalogue.CurrentPassword.Number =>
				this.nodeLogin.HandleAsync(envelope, cancellationToken),
			SetParameter
			{
				ParameterNumber: var number
			} when number == RouterParameterCatalogue.Level1PasswordNumber =>
				this.level1PasswordModification.HandleAsync(envelope, cancellationToken),
			_ => ValueTask.FromResult<Envelope?>(null)
		};
	}
}
