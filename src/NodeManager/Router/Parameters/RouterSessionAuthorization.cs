using System.Collections.Concurrent;

namespace NodeManager.Router.Parameters;

public interface IRouterSessionAuthorization
{
	bool IsAuthorized(string browserSessionIdentifier);

	void TrackLogOn(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier);

	void TrackLogOff(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier);

	void Observe(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier,
		RouterParameterRequestStatus status);
}

public sealed class RouterSessionAuthorization : IRouterSessionAuthorization
{
	private readonly ConcurrentDictionary<string, byte> authorizedBrowserSessions = [];
	private readonly ConcurrentDictionary<RouterParameterRequestStatusIdentifier, PendingTransaction>
		pendingTransactions = [];

	public bool IsAuthorized(string browserSessionIdentifier)
	{
		ArgumentException.ThrowIfNullOrEmpty(browserSessionIdentifier);
		return this.authorizedBrowserSessions.ContainsKey(browserSessionIdentifier);
	}

	public void TrackLogOn(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier)
	{
		this.Track(browserSessionIdentifier, transactionIdentifier, PendingTransactionKind.LogOn);
	}

	public void TrackLogOff(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier)
	{
		this.Track(browserSessionIdentifier, transactionIdentifier, PendingTransactionKind.LogOff);
	}

	public void Observe(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier,
		RouterParameterRequestStatus status)
	{
		ArgumentException.ThrowIfNullOrEmpty(browserSessionIdentifier);
		ArgumentNullException.ThrowIfNull(transactionIdentifier);
		ArgumentNullException.ThrowIfNull(status);

		if (!this.pendingTransactions.TryGetValue(transactionIdentifier, out var pendingTransaction) ||
			!string.Equals(
				pendingTransaction.BrowserSessionIdentifier,
				browserSessionIdentifier,
				StringComparison.Ordinal))
		{
			return;
		}

		if (pendingTransaction.Kind == PendingTransactionKind.LogOn &&
			status is LoggedOnNodeLoginStatus)
		{
			this.authorizedBrowserSessions.TryAdd(browserSessionIdentifier, 0);
			this.pendingTransactions.TryRemove(transactionIdentifier, out _);
		}
		else if (pendingTransaction.Kind == PendingTransactionKind.LogOff &&
			status is LoggedOffNodeLoginStatus)
		{
			this.authorizedBrowserSessions.TryRemove(browserSessionIdentifier, out _);
			this.pendingTransactions.TryRemove(transactionIdentifier, out _);
		}
		else if (status is not PendingNodeLoginStatus and not PendingNodeLogoffStatus)
		{
			this.pendingTransactions.TryRemove(transactionIdentifier, out _);
		}
	}

	private void Track(
		string browserSessionIdentifier,
		RouterParameterRequestStatusIdentifier transactionIdentifier,
		PendingTransactionKind kind)
	{
		ArgumentException.ThrowIfNullOrEmpty(browserSessionIdentifier);
		ArgumentNullException.ThrowIfNull(transactionIdentifier);

		this.pendingTransactions[transactionIdentifier] =
			new PendingTransaction(browserSessionIdentifier, kind);
	}

	private sealed record PendingTransaction(
		string BrowserSessionIdentifier,
		PendingTransactionKind Kind);

	private enum PendingTransactionKind
	{
		LogOn,
		LogOff
	}
}
