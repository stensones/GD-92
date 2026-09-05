namespace NodeManager.Router.Parameters;

public sealed class NodeLoginRetryDelay : INodeLoginRetryDelay
{
	public Task WaitAsync(
		NodeLoginNoAcknowledgementTimeout timeout,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(timeout);

		return Task.Delay(timeout.Duration, cancellationToken);
	}
}
