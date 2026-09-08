namespace NodeManager.Router.Parameters;

public interface IManagementTransactionRetryDelay
{
	Task WaitAsync(
		ManagementTransactionNoAcknowledgementTimeout timeout,
		CancellationToken cancellationToken);
}
