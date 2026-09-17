using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Router.Parameters;

public sealed class ManagementTransactions(
	ManagementTransactionRetryPolicy retryPolicy,
	IServiceScopeFactory serviceScopeFactory,
	IManagementTransactionRetryDelay retryDelay,
	IHostApplicationLifetime applicationLifetime,
	ILogger<ManagementTransactions> logger,
	IManagementTransactionUiNotifier? uiNotifier = null) :
	IManagementTransactionService,
	IUserAgentIngressReceiver
{
	private const ushort MaximumSequenceNumber = 32767;
	private readonly ManagementTransactionRetryPolicy retryPolicy = retryPolicy ??
		throw new ArgumentNullException(nameof(retryPolicy));
	private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ??
		throw new ArgumentNullException(nameof(serviceScopeFactory));
	private readonly IManagementTransactionRetryDelay retryDelay = retryDelay ??
		throw new ArgumentNullException(nameof(retryDelay));
	private readonly CancellationToken applicationStopping = applicationLifetime?.ApplicationStopping ??
		throw new ArgumentNullException(nameof(applicationLifetime));
	private readonly ILogger<ManagementTransactions> logger = logger ??
		throw new ArgumentNullException(nameof(logger));
	private readonly IManagementTransactionUiNotifier uiNotifier =
		uiNotifier ?? new NullManagementTransactionUiNotifier();
	private readonly object synchronizationLock = new();
	private readonly Dictionary<UniqueSystemWideReference, RouterParameterRequestStatus> statuses = [];
	private readonly Dictionary<CommunicationsAddress, HashSet<ushort>> activeSequencesByDestination = [];
	private readonly Dictionary<CommunicationsAddress, ushort> nextSequenceByDestination = [];
	private readonly Dictionary<UniqueSystemWideReference, ManagementTransactionUiRecipient>
		uiRecipients = [];
	private readonly HashSet<UniqueSystemWideReference> notifiedUiTransactions = [];

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
			await this.SubmitToRouterAsync(envelope, cancellationToken);
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
			if (this.TryRecordDeliveryFailure(statusIdentifier))
			{
				await this.NotifyTerminalStatusAsync(statusIdentifier);
			}

			return statusIdentifier;
		}

		_ = this.RetryUntilTerminalAsync(statusIdentifier, envelope);
		return statusIdentifier;
	}

	public RouterParameterRequestStatus? GetStatus(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		lock (this.synchronizationLock)
		{
			return this.statuses.GetValueOrDefault(statusIdentifier.USWR);
		}
	}

	public async Task RegisterUiRecipientAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		ManagementTransactionUiRecipient recipient,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);
		ArgumentNullException.ThrowIfNull(recipient);
		cancellationToken.ThrowIfCancellationRequested();

		ManagementTransactionCompletion? completion;
		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(statusIdentifier.USWR, out var status))
			{
				throw new InvalidOperationException("The Management Transaction is unknown.");
			}

			this.uiRecipients[statusIdentifier.USWR] = recipient;
			completion = this.CreateTerminalNotification(statusIdentifier.USWR, status);
		}

		if (completion is not null)
		{
			await this.NotifyUiAsync(completion);
		}
	}

	public async Task<RouterParameterRequestStatus> WaitForCompletionAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(statusIdentifier);

		while (this.IsActive(statusIdentifier))
		{
			await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
		}

		return this.GetStatus(statusIdentifier) ??
			throw new InvalidOperationException("The Management Transaction is unknown.");
	}

	public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);
		cancellationToken.ThrowIfCancellationRequested();

		var handled = this.TryCompleteParameterResponse(envelope);
		if (!handled)
		{
			handled = this.TryCompleteAcknowledgement(envelope);
			if (!handled)
			{
				handled = this.TryCompleteNegativeAcknowledgement(envelope);
			}
		}

		return handled
			? this.NotifyTerminalStatusAsync(new RouterParameterRequestStatusIdentifier(
				ReferenceFromResponse(envelope)))
			: Task.CompletedTask;
	}

	private RouterParameterRequestStatusIdentifier Reserve(ManagementTransactionRequest request)
	{
		return request.Kind switch
		{
			ManagementTransactionKind.ParameterRequest => this.ReserveTransaction(
				request.Source,
				request.Destination,
				static identifier => new PendingRouterParameterRequestStatus(identifier)),
			ManagementTransactionKind.ParameterModification => this.ReserveTransaction(
				request.Source,
				request.Destination,
				static identifier => new PendingParameterModificationStatus(identifier)),
			ManagementTransactionKind.NodeLogin when request.NodeLoginUserAgentAddress is not null =>
				this.ReserveTransaction(
					request.Source,
					request.Destination,
					identifier => new PendingNodeLoginStatus(identifier, request.NodeLoginUserAgentAddress)),
			ManagementTransactionKind.NodeLogoff => this.ReserveTransaction(
				request.Source,
				request.Destination,
				static identifier => new PendingNodeLogoffStatus(identifier)),
			ManagementTransactionKind.NodeLogin => throw new ArgumentException(
				"A Node Login Management Transaction requires its User-Agent address.",
				nameof(request)),
			_ => throw new ArgumentOutOfRangeException(nameof(request))
		};
	}

	private RouterParameterRequestStatusIdentifier ReserveTransaction(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		Func<RouterParameterRequestStatusIdentifier, RouterParameterRequestStatus> createPendingStatus)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);
		ArgumentNullException.ThrowIfNull(createPendingStatus);

		lock (this.synchronizationLock)
		{
			var activeSequences = this.GetActiveSequences(destination);
			var candidate = this.nextSequenceByDestination.GetValueOrDefault(destination);

			for (var attempts = 0; attempts <= MaximumSequenceNumber; attempts++)
			{
				if (!activeSequences.Contains(candidate))
				{
					var sequenceNumber = SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(candidate));
					var uswr = new UniqueSystemWideReference(source, destination, sequenceNumber);
					var identifier = new RouterParameterRequestStatusIdentifier(uswr);

					activeSequences.Add(candidate);
					this.statuses.Add(uswr, createPendingStatus(identifier));
					this.nextSequenceByDestination[destination] =
						candidate == MaximumSequenceNumber ? (ushort)0 : (ushort)(candidate + 1);

					return identifier;
				}

				candidate = candidate == MaximumSequenceNumber ? (ushort)0 : (ushort)(candidate + 1);
			}
		}

		throw new InvalidOperationException("All sequence numbers for the destination are pending.");
	}

	private async Task SubmitToRouterAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		using var scope = this.serviceScopeFactory.CreateScope();
		var routerIngress = scope.ServiceProvider.GetRequiredService<IRouterIngress>();
		await routerIngress.SubmitAsync(envelope, cancellationToken);
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

				if (!this.IsActive(statusIdentifier))
				{
					return;
				}

				if (this.IsAwaitingFinalResponse(statusIdentifier))
				{
					if (this.TryTimeout(statusIdentifier))
					{
						await this.NotifyTerminalStatusAsync(statusIdentifier);
					}

					return;
				}

				try
				{
					await this.SubmitToRouterAsync(envelope, this.applicationStopping);
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
					if (this.TryRecordDeliveryFailure(statusIdentifier))
					{
						await this.NotifyTerminalStatusAsync(statusIdentifier);
					}

					return;
				}
			}

			await this.retryDelay.WaitAsync(
				this.retryPolicy.NoAcknowledgementTimeout,
				this.applicationStopping);

			if (this.IsActive(statusIdentifier))
			{
				if (this.TryTimeout(statusIdentifier))
				{
					await this.NotifyTerminalStatusAsync(statusIdentifier);
				}
			}
		}
		catch (OperationCanceledException) when (this.applicationStopping.IsCancellationRequested)
		{
		}
	}

	private bool IsActive(RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		lock (this.synchronizationLock)
		{
			return this.statuses.TryGetValue(statusIdentifier.USWR, out var status) &&
				status is PendingRouterParameterRequestStatus or DeferredRouterParameterRequestStatus or
					PendingNodeLoginStatus or PendingNodeLogoffStatus or
					PendingParameterModificationStatus;
		}
	}

	private bool IsAwaitingFinalResponse(RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		lock (this.synchronizationLock)
		{
			return this.statuses.GetValueOrDefault(statusIdentifier.USWR) is
				DeferredRouterParameterRequestStatus;
		}
	}

	private bool TryCompleteParameterResponse(Envelope envelope)
	{
		if (envelope.Contents is not Parameter ||
			envelope.Destinations.Addresses.Count != 1)
		{
			return false;
		}

		var uswr = ReferenceFromResponse(envelope);
		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(uswr, out var status) ||
				status is not (PendingRouterParameterRequestStatus or
					DeferredRouterParameterRequestStatus))
			{
				return false;
			}

			this.statuses[uswr] = new ReceivedRouterParameterRequestStatus(
				new RouterParameterRequestStatusIdentifier(uswr),
				((Parameter)envelope.Contents).MoreValues,
				((Parameter)envelope.Contents).ParameterValue);
			this.ReleaseSequence(uswr);
			return true;
		}
	}

	private bool TryCompleteAcknowledgement(Envelope envelope)
	{
		if (envelope.Contents is not Acknowledgement ||
			envelope.Destinations.Addresses.Count != 1)
		{
			return false;
		}

		var uswr = ReferenceFromResponse(envelope);
		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(uswr, out var status))
			{
				return false;
			}

			switch (status)
			{
				case PendingNodeLoginStatus pendingNodeLogin:
					this.statuses[uswr] = new LoggedOnNodeLoginStatus(
						new RouterParameterRequestStatusIdentifier(uswr),
						pendingNodeLogin.UserAgentAddress);
					break;
				case PendingNodeLogoffStatus:
					this.statuses[uswr] = new LoggedOffNodeLoginStatus(
						new RouterParameterRequestStatusIdentifier(uswr));
					break;
				case PendingParameterModificationStatus:
					this.statuses[uswr] = new AcknowledgedParameterModificationStatus(
						new RouterParameterRequestStatusIdentifier(uswr));
					break;
				default:
					return false;
			}

			this.ReleaseSequence(uswr);
			return true;
		}
	}

	private bool TryCompleteNegativeAcknowledgement(Envelope envelope)
	{
		if (envelope.Contents is not NegativeAcknowledgement negativeAcknowledgement ||
			envelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested ||
			envelope.Destinations.Addresses.Count != 1)
		{
			return false;
		}

		var uswr = ReferenceFromResponse(envelope);
		if (negativeAcknowledgement.Destinations.Addresses.Count != 1 ||
			negativeAcknowledgement.Destinations.Addresses[0] != uswr.Destination)
		{
			return false;
		}

		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(uswr, out var status))
			{
				return false;
			}

			switch (status)
			{
				case PendingNodeLoginStatus pendingNodeLogin:
					this.statuses[uswr] =
						negativeAcknowledgement.ReasonCode.ParameterReasonCode == ParameterReasonCode.InvalidPassword
							? new InvalidPasswordNodeLoginStatus(
								new RouterParameterRequestStatusIdentifier(uswr),
								pendingNodeLogin.UserAgentAddress)
							: new RejectedNodeLoginStatus(
								new RouterParameterRequestStatusIdentifier(uswr),
								pendingNodeLogin.UserAgentAddress);
					break;
				case PendingRouterParameterRequestStatus:
				case PendingParameterModificationStatus:
					this.statuses[uswr] =
						negativeAcknowledgement.ReasonCode.GeneralReasonCode ==
						GeneralReasonCode.WaitForAcknowledgement
							? new DeferredRouterParameterRequestStatus(
								new RouterParameterRequestStatusIdentifier(uswr))
							: new RejectedRouterParameterRequestStatus(
								new RouterParameterRequestStatusIdentifier(uswr),
								negativeAcknowledgement.ReasonCode);
					break;
				case PendingNodeLogoffStatus:
					this.statuses[uswr] = new RejectedRouterParameterRequestStatus(
						new RouterParameterRequestStatusIdentifier(uswr),
						negativeAcknowledgement.ReasonCode);
					break;
				default:
					return false;
			}

			this.ReleaseSequence(uswr);
			return true;
		}
	}

	private bool TryTimeout(RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(statusIdentifier.USWR, out var status) ||
				status is not (PendingRouterParameterRequestStatus or
					DeferredRouterParameterRequestStatus or PendingNodeLoginStatus or
					PendingNodeLogoffStatus or PendingParameterModificationStatus))
			{
				return false;
			}

			this.statuses[statusIdentifier.USWR] = status switch
			{
				PendingNodeLoginStatus pendingNodeLogin => new TimedOutNodeLoginStatus(
					statusIdentifier,
					pendingNodeLogin.UserAgentAddress),
				_ => new TimedOutRouterParameterRequestStatus(statusIdentifier)
			};
			this.ReleaseSequence(statusIdentifier.USWR);
			return true;
		}
	}

	private bool TryRecordDeliveryFailure(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		lock (this.synchronizationLock)
		{
			if (!this.statuses.TryGetValue(statusIdentifier.USWR, out var status) ||
				status is not (PendingRouterParameterRequestStatus or
					PendingNodeLoginStatus or PendingNodeLogoffStatus or
					PendingParameterModificationStatus))
			{
				return false;
			}

			this.statuses[statusIdentifier.USWR] =
				new DeliveryFailedRouterParameterRequestStatus(statusIdentifier);
			this.ReleaseSequence(statusIdentifier.USWR);
			return true;
		}
	}

	private void ReleaseSequence(UniqueSystemWideReference uswr)
	{
		this.GetActiveSequences(uswr.Destination).Remove(uswr.SequenceNumber.Value);
	}

	private async Task NotifyTerminalStatusAsync(
		RouterParameterRequestStatusIdentifier statusIdentifier)
	{
		ManagementTransactionCompletion? completion;
		lock (this.synchronizationLock)
		{
			completion = this.statuses.TryGetValue(statusIdentifier.USWR, out var status)
				? this.CreateTerminalNotification(statusIdentifier.USWR, status)
				: null;
		}

		if (completion is not null)
		{
			await this.NotifyUiAsync(completion);
		}
	}

	private ManagementTransactionCompletion? CreateTerminalNotification(
		UniqueSystemWideReference uswr,
		RouterParameterRequestStatus status)
	{
		if (status is PendingRouterParameterRequestStatus or DeferredRouterParameterRequestStatus or
			PendingNodeLoginStatus or PendingNodeLogoffStatus or PendingParameterModificationStatus ||
			!this.uiRecipients.TryGetValue(uswr, out var recipient) ||
			!this.notifiedUiTransactions.Add(uswr))
		{
			return null;
		}

		return new ManagementTransactionCompletion(
			recipient.BrowserSessionIdentifier,
			recipient.RequestIdentifier,
			status.Identifier.ToString());
	}

	private async Task NotifyUiAsync(ManagementTransactionCompletion completion)
	{
		try
		{
			await this.uiNotifier.NotifyAsync(completion, CancellationToken.None);
		}
		catch (Exception exception)
		{
			this.logger.LogWarning(
				exception,
				"Could not notify the browser that Management Transaction {StatusIdentifier} completed.",
				completion.TransactionIdentifier);
		}
	}

	private HashSet<ushort> GetActiveSequences(CommunicationsAddress destination)
	{
		if (this.activeSequencesByDestination.TryGetValue(destination, out var activeSequences))
		{
			return activeSequences;
		}

		activeSequences = [];
		this.activeSequencesByDestination.Add(destination, activeSequences);
		return activeSequences;
	}

	private static UniqueSystemWideReference ReferenceFromResponse(Envelope envelope)
	{
		return new UniqueSystemWideReference(
			envelope.Destinations.Addresses[0],
			envelope.Source,
			envelope.AcknowledgementAndSequence.SequenceNumber);
	}
}
