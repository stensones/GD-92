using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ;

public sealed class RabbitMqRouterIngress : IRouterIngress
{
	private readonly IngressEnvelopeTransport transport;
	private readonly CommunicationsAddress localRouter;

	public RabbitMqRouterIngress(
		IMessageBus messageBus,
		CommunicationsAddress localRouter)
	{
		ArgumentNullException.ThrowIfNull(messageBus);
		ArgumentNullException.ThrowIfNull(localRouter);

		this.transport = new IngressEnvelopeTransport(messageBus);
		this.localRouter = localRouter;
	}

	public async Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		await this.transport.SubmitAsync(
			envelope,
			RouterIngressEndpoint.From(this.localRouter),
			wireValue => new RouterIngressTransportMessage(wireValue),
			cancellationToken);
	}
}
