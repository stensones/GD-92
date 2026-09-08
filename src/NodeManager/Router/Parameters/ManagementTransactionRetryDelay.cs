namespace NodeManager.Router.Parameters;

public sealed class ManagementTransactionRetryDelay : IManagementTransactionRetryDelay
{
	public Task WaitAsync(
		ManagementTransactionNoAcknowledgementTimeout timeout,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(timeout);

		return Task.Delay(timeout.Duration, cancellationToken);
	}
}
