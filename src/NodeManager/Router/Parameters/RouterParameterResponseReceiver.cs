using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

internal sealed class RouterParameterResponseReceiver(
	IPendingDeliveryRegistry pendingDeliveries) : IUserAgentIngressReceiver
{
	private readonly IPendingDeliveryRegistry pendingDeliveries = pendingDeliveries ??
		throw new ArgumentNullException(nameof(pendingDeliveries));

	public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		this.pendingDeliveries.TryCompleteParameterResponse(envelope);
		return Task.CompletedTask;
	}
}
