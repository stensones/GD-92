using NodeManager.Router.Parameters;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace NodeManager.Router.Participants;

public sealed class NodeManagerRouterIngress : IRouterIngress
{
	private readonly RabbitMqRouterIngress routerIngress;

	public NodeManagerRouterIngress(
		IMessageBus messageBus,
		RouterParameterRequestSettings settings)
	{
		ArgumentNullException.ThrowIfNull(messageBus);
		ArgumentNullException.ThrowIfNull(settings);

		this.routerIngress = new RabbitMqRouterIngress(messageBus, settings.LocalRouter);
	}

	public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		return this.routerIngress.SubmitAsync(envelope, cancellationToken);
	}
}
