using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class ManagementTransactionService(
	IManagementTransactionRegistry transactions,
	ManagementTransactionRetryPolicy retryPolicy,
	IRouterIngress routerIngress,
	IServiceScopeFactory serviceScopeFactory,
	IManagementTransactionRetryDelay retryDelay,
	IHostApplicationLifetime applicationLifetime,
	ILogger<ManagementTransactionService> logger) : IManagementTransactionService
{
	private readonly IManagementTransactionRegistry transactions = transactions ??
		throw new ArgumentNullException(nameof(transactions));
	private readonly ManagementTransactionRetryPolicy retryPolicy = retryPolicy ??
		throw new ArgumentNullException(nameof(retryPolicy));
	private readonly IRouterIngress routerIngress = routerIngress ??
		throw new ArgumentNullException(nameof(routerIngress));
	private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ??
		throw new ArgumentNullException(nameof(serviceScopeFactory));
	private readonly IManagementTransactionRetryDelay retryDelay = retryDelay ??
		throw new ArgumentNullException(nameof(retryDelay));
	private readonly CancellationToken applicationStopping = applicationLifetime?.ApplicationStopping ??
		throw new ArgumentNullException(nameof(applicationLifetime));
	private readonly ILogger<ManagementTransactionService> logger = logger ??
		throw new ArgumentNullException(nameof(logger));

	public async Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
		ManagementTransactionRequest request,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(request.CreateEnvelope);

		var statusIdentifier = this.Reserve(request);
		var envelope = request.CreateEnvelope(statusIdentifier.USWR.SequenceNumber);

		try
		{
			await this.routerIngress.SubmitAsync(envelope, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception exception)
		{
			this.logger.LogError(
				exception,
				"Router Ingress failed to submit Management Transaction {StatusIdentifier}.",
				statusIdentifier);
			this.transactions.TryRecordDeliveryFailure(statusIdentifier);
			return statusIdentifier;
		}

		_ = this.RetryUntilTerminalAsync(statusIdentifier, envelope);
		return statusIdentifier;
	}

	public async Task<RouterParameterRequestStatus> WaitForCompletionAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		while (this.transactions.IsActive(statusIdentifier))
		{
			await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
		}

		return this.transactions.GetStatus(statusIdentifier) ??
			throw new InvalidOperationException("The Management Transaction is unknown.");
	}

	private RouterParameterRequestStatusIdentifier Reserve(ManagementTransactionRequest request)
	{
		return request.Kind switch
		{
			ManagementTransactionKind.ParameterRequest => this.transactions.ReserveParameterRequest(
				request.Source,
				request.Destination),
			ManagementTransactionKind.NodeLogin when request.NodeLoginUserAgentAddress is not null =>
				this.transactions.ReserveNodeLogin(
					request.Source,
					request.Destination,
					request.NodeLoginUserAgentAddress),
			ManagementTransactionKind.NodeLogoff => this.transactions.ReserveNodeLogoff(
				request.Source,
				request.Destination),
			ManagementTransactionKind.NodeLogin => throw new ArgumentException(
				"A Node Login Management Transaction requires its User-Agent address.",
				nameof(request)),
			_ => throw new ArgumentOutOfRangeException(nameof(request))
		};
	}

	private async Task RetryUntilTerminalAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		Envelope envelope)
	{
		try
		{
			for (var sends = 1; sends < this.retryPolicy.TotalSends.Value.Value; sends++)
			{
				await this.retryDelay.WaitAsync(
					this.retryPolicy.NoAcknowledgementTimeout,
					this.applicationStopping);

				if (!this.transactions.IsActive(statusIdentifier))
				{
					return;
				}

				if (this.transactions.IsAwaitingFinalResponse(statusIdentifier))
				{
					this.transactions.TryTimeout(statusIdentifier);
					return;
				}

				try
				{
					using var scope = this.serviceScopeFactory.CreateScope();
					var retryIngress = scope.ServiceProvider.GetRequiredService<IRouterIngress>();
					await retryIngress.SubmitAsync(envelope, this.applicationStopping);
				}
				catch (OperationCanceledException) when (this.applicationStopping.IsCancellationRequested)
				{
					return;
				}
				catch (Exception exception)
				{
					this.logger.LogError(
						exception,
						"Router Ingress failed to retry Management Transaction {StatusIdentifier}.",
						statusIdentifier);
					this.transactions.TryRecordDeliveryFailure(statusIdentifier);
					return;
				}
			}

			await this.retryDelay.WaitAsync(
				this.retryPolicy.NoAcknowledgementTimeout,
				this.applicationStopping);

			if (this.transactions.IsActive(statusIdentifier))
			{
				this.transactions.TryTimeout(statusIdentifier);
			}
		}
		catch (OperationCanceledException) when (this.applicationStopping.IsCancellationRequested)
		{
		}
	}
}
