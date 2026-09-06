namespace NodeManager.Router.Participants;

public sealed record InventoryScanSummary(
	int DiscoveredParticipantCount,
	int TimeoutCount,
	int DeliveryFailureCount,
	IReadOnlyDictionary<string, int> NegativeAcknowledgements)
{
	public static InventoryScanSummary Empty { get; } = new(
		DiscoveredParticipantCount: 0,
		TimeoutCount: 0,
		DeliveryFailureCount: 0,
		NegativeAcknowledgements: new Dictionary<string, int>());
}
