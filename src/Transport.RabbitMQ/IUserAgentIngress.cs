using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public interface IUserAgentIngress
{
	Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken);
}
