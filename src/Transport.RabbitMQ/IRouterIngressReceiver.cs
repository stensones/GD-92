using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public interface IRouterIngressReceiver
{
	Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken);
}
