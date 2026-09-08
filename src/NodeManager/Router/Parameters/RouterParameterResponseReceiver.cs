using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class RouterParameterResponseReceiver(
	IManagementTransactionRegistry transactions) : IUserAgentIngressReceiver
{
	private readonly IManagementTransactionRegistry transactions = transactions ??
		throw new ArgumentNullException(nameof(transactions));

	public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		if (!this.transactions.TryCompleteParameterResponse(envelope))
		{
			if (!this.transactions.TryCompleteAcknowledgement(envelope))
			{
				this.transactions.TryCompleteNegativeAcknowledgement(envelope);
			}
		}

		return Task.CompletedTask;
	}
}
