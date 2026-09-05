using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class RouterParameterResponseReceiver(
	IPendingDeliveryRegistry pendingDeliveries) : IUserAgentIngressReceiver
{
	private readonly IPendingDeliveryRegistry pendingDeliveries = pendingDeliveries ??
		throw new ArgumentNullException(nameof(pendingDeliveries));

	public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (!this.pendingDeliveries.TryCompleteParameterResponse(envelope))
		{
			if (!this.pendingDeliveries.TryCompleteAcknowledgement(envelope))
			{
				this.pendingDeliveries.TryCompleteNegativeAcknowledgement(envelope);
			}
		}

		return Task.CompletedTask;
	}
}
