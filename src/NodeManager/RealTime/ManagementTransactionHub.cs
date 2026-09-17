using Microsoft.AspNetCore.SignalR;
using NodeManager.Router.Parameters;

namespace NodeManager.RealTime;

public interface IManagementTransactionClient
{
	Task TransactionCompleted(ManagementTransactionCompletion notification);
}

public sealed class ManagementTransactionHub :
	Hub<IManagementTransactionClient>
{
	public override async Task OnConnectedAsync()
	{
		var httpContext = this.Context.GetHttpContext() ?? throw new HubException(
			"A browser session is required to receive management transaction notifications.");
		var browserSession = BrowserSessionIdentifier.Get(httpContext);

		await this.Groups.AddToGroupAsync(
			this.Context.ConnectionId,
			BrowserSessionIdentifier.GroupName(browserSession));
		await base.OnConnectedAsync();
	}
}

public sealed class SignalRManagementTransactionUiNotifier(
	IHubContext<ManagementTransactionHub, IManagementTransactionClient> hubContext) :
	IManagementTransactionUiNotifier
{
	private readonly IHubContext<ManagementTransactionHub, IManagementTransactionClient> hubContext =
		hubContext ?? throw new ArgumentNullException(nameof(hubContext));

	public Task NotifyAsync(
		ManagementTransactionCompletion notification,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(notification);
		cancellationToken.ThrowIfCancellationRequested();

		return this.hubContext.Clients
			.Group(BrowserSessionIdentifier.GroupName(notification.BrowserSessionIdentifier))
			.TransactionCompleted(notification);
	}
}
