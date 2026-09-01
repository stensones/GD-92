using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RabbitMqRouterIngress : IRouterIngress
{
	private readonly IMessageBus messageBus;
	private readonly CommunicationsAddress localRouter;

	public RabbitMqRouterIngress(
		IMessageBus messageBus,
		CommunicationsAddress localRouter)
	{
		ArgumentNullException.ThrowIfNull(messageBus);
		ArgumentNullException.ThrowIfNull(localRouter);

		this.messageBus = messageBus;
		this.localRouter = localRouter;
	}

	public async Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		var message = new RouterIngressTransportMessage(envelope.ToWireValue());
		await messageBus
			.EndpointFor(RouterIngressEndpoint.From(localRouter))
			.SendAsync(message)
			.AsTask()
			.WaitAsync(cancellationToken);
	}
}
