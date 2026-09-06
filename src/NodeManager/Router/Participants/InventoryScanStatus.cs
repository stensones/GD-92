namespace NodeManager.Router.Participants;

public sealed record InventoryScanStatus(
	int CompletedProbeCount,
	IReadOnlyList<InventoryParticipant> Participants,
	InventoryScanSummary Summary);
