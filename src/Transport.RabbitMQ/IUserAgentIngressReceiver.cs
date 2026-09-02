using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public interface IUserAgentIngressReceiver
{
	Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken);
}
