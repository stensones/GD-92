namespace NodeManager.Router.Parameters;

public interface INodeLoginRetryDelay
{
	Task WaitAsync(
		NodeLoginNoAcknowledgementTimeout timeout,
		CancellationToken cancellationToken);
}
