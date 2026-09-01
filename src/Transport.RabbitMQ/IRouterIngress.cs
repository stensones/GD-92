using Stensones.GD92.Messages;

namespace Stensones.GD92.Transport.RabbitMQ;

public interface IRouterIngress
{
	Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken);
}
