using Microsoft.Extensions.DependencyInjection;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class NodeLoginRetryScheduler(
	NodeLoginRetryPolicy retryPolicy,
	IPendingDeliveryRegistry pendingDeliveries,
	IServiceScopeFactory serviceScopeFactory,
	INodeLoginRetryDelay retryDelay,
	CancellationToken applicationStopping) : INodeLoginRetryScheduler
{
	private readonly NodeLoginRetryPolicy retryPolicy = retryPolicy ??
		throw new ArgumentNullException(nameof(retryPolicy));
	private readonly IPendingDeliveryRegistry pendingDeliveries = pendingDeliveries ??
		throw new ArgumentNullException(nameof(pendingDeliveries));
	private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ??
		throw new ArgumentNullException(nameof(serviceScopeFactory));
	private readonly INodeLoginRetryDelay retryDelay = retryDelay ??
		throw new ArgumentNullException(nameof(retryDelay));

	public void Schedule(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);
		ArgumentNullException.ThrowIfNull(envelope);

		_ = this.RetryUntilTerminalAsync(statusIdentifier, envelope);
	}

	public async Task RetryUntilTerminalAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		Envelope envelope)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);
		ArgumentNullException.ThrowIfNull(envelope);

		try
		{
			for (var sends = 1; sends < this.retryPolicy.TotalSends.Value.Value; sends++)
			{
				await this.retryDelay.WaitAsync(
					this.retryPolicy.NoAcknowledgementTimeout,
					applicationStopping);

				if (!this.pendingDeliveries.IsPending(statusIdentifier))
				{
					return;
				}

				using var scope = this.serviceScopeFactory.CreateScope();
				var routerIngress = scope.ServiceProvider.GetRequiredService<IRouterIngress>();
				await routerIngress.SubmitAsync(envelope, applicationStopping);
			}

			await this.retryDelay.WaitAsync(
				this.retryPolicy.NoAcknowledgementTimeout,
				applicationStopping);

			if (this.pendingDeliveries.IsPending(statusIdentifier))
			{
				if (!this.pendingDeliveries.TryTimeoutNodeLogin(statusIdentifier))
				{
					this.pendingDeliveries.TryTimeoutParameterRequest(statusIdentifier);
				}
			}
		}
		catch (OperationCanceledException) when (applicationStopping.IsCancellationRequested)
		{
		}
	}
}
