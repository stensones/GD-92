using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Router;

internal sealed class RouterIngressReceiver : IRouterIngressReceiver
{
	public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		return Task.CompletedTask;
	}
}
