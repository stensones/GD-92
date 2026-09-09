using Microsoft.Extensions.Configuration;

namespace NodeManager.Router.Participants;

public sealed class InventoryScanSettings
{
	private const int DefaultMaximumConcurrentProbes = 8;
	private const int TotalParticipantPorts = 63;

	private InventoryScanSettings(int maximumConcurrentProbes)
	{
		this.MaximumConcurrentProbes = maximumConcurrentProbes;
	}

	public int MaximumConcurrentProbes { get; }

	public static InventoryScanSettings FromConfiguration(IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var configuredValue = configuration["InventoryScan:MaximumConcurrentProbes"];
		if (configuredValue is null)
		{
			return new InventoryScanSettings(DefaultMaximumConcurrentProbes);
		}

		if (!int.TryParse(configuredValue, out var maximumConcurrentProbes) ||
			maximumConcurrentProbes is < 1 or > TotalParticipantPorts)
		{
			throw new InvalidOperationException(
				"InventoryScan:MaximumConcurrentProbes must be an integer from 1 to 63.");
		}

		return new InventoryScanSettings(maximumConcurrentProbes);
	}
}
