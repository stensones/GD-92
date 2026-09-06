namespace NodeManager.Router.Participants;

public sealed record InventoryParticipant(
	byte Port,
	string Kind,
	string? AgentType);
