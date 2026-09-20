namespace NodeManager.Router.Participants;

public sealed record InventoryScanProgressUpdate(
	string BrowserSessionIdentifier,
	Guid RequestIdentifier,
	string ScanIdentifier,
	InventoryScanStatus Status);

public interface IInventoryScanUiNotifier
{
	Task NotifyAsync(
		InventoryScanProgressUpdate update,
		CancellationToken cancellationToken);
}

internal sealed class NullInventoryScanUiNotifier : IInventoryScanUiNotifier
{
	public Task NotifyAsync(
		InventoryScanProgressUpdate update,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(update);
		cancellationToken.ThrowIfCancellationRequested();
		return Task.CompletedTask;
	}
}
