namespace NodeManager.Router.Parameters;

public sealed record ManagementTransactionUiRecipient(
	string BrowserSessionIdentifier,
	Guid RequestIdentifier)
{
	public const string RequestIdentifierHeaderName = "X-GD92-Management-Request-Id";

	public static bool TryCreate(
		HttpContext context,
		out ManagementTransactionUiRecipient? recipient,
		out string? validationError)
	{
		ArgumentNullException.ThrowIfNull(context);

		var requestIdentifierValues = context.Request.Headers[RequestIdentifierHeaderName];
		if (requestIdentifierValues.Count == 0)
		{
			recipient = null;
			validationError = null;
			return true;
		}

		if (requestIdentifierValues.Count != 1 ||
			!Guid.TryParseExact(requestIdentifierValues[0], "N", out var requestIdentifier))
		{
			recipient = null;
			validationError =
				$"{RequestIdentifierHeaderName} must contain one GUID in N format.";
			return false;
		}

		recipient = new ManagementTransactionUiRecipient(
			RealTime.BrowserSessionIdentifier.Get(context),
			requestIdentifier);
		validationError = null;
		return true;
	}
}

public sealed record ManagementTransactionCompletion(
	string BrowserSessionIdentifier,
	Guid RequestIdentifier,
	string TransactionIdentifier);

public interface IManagementTransactionUiNotifier
{
	Task NotifyAsync(
		ManagementTransactionCompletion notification,
		CancellationToken cancellationToken);
}

internal sealed class NullManagementTransactionUiNotifier : IManagementTransactionUiNotifier
{
	public Task NotifyAsync(
		ManagementTransactionCompletion notification,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(notification);
		cancellationToken.ThrowIfCancellationRequested();
		return Task.CompletedTask;
	}
}
