using Aspire.Hosting;

namespace Stensones.GD92.StationEnd.Tests.Integration;

internal static class StationEndApplicationStartup
{
	private static readonly TimeSpan ResourceReadinessTimeout = TimeSpan.FromMinutes(1);

	public static async Task WaitForHealthyAsync(
		DistributedApplication application,
		params string[] resourceNames)
	{
		ArgumentNullException.ThrowIfNull(application);
		ArgumentNullException.ThrowIfNull(resourceNames);

		using var timeout = new CancellationTokenSource(ResourceReadinessTimeout);
		try
		{
			foreach (var resourceName in resourceNames)
			{
				await application.ResourceNotifications.WaitForResourceHealthyAsync(
					resourceName,
					timeout.Token).WaitAsync(timeout.Token);
			}
		}
		catch (OperationCanceledException exception) when (timeout.IsCancellationRequested)
		{
			throw new TimeoutException(
				$"Timed out after {ResourceReadinessTimeout} waiting for Station End resources " +
				$"[{string.Join(", ", resourceNames)}] to become healthy.",
				exception);
		}
	}
}
