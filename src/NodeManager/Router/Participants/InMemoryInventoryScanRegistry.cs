using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Participants;

public sealed class InMemoryInventoryScanRegistry : IInventoryScanRegistry
{
	private readonly object synchronizationLock = new();
	private readonly Dictionary<Guid, InventoryScanStatus> statuses = [];

	public InventoryScanStatusIdentifier Start()
	{
		var identifier = InventoryScanStatusIdentifier.Create();
		var status = new InventoryScanStatus(
			CompletedProbeCount: 0,
			Participants:
			[
				new InventoryParticipant(
					Port: 0,
					Kind: "router",
					AgentType: null)
			],
			Summary: InventoryScanSummary.Empty);

		lock (this.synchronizationLock)
		{
			this.statuses.Add(identifier.Value, status);
		}

		return identifier;
	}

	public InventoryScanStatus? Get(InventoryScanStatusIdentifier identifier)
	{
		ArgumentNullException.ThrowIfNull(identifier);

		lock (this.synchronizationLock)
		{
			return this.statuses.GetValueOrDefault(identifier.Value);
		}
	}

	public void RecordTimeout(InventoryScanStatusIdentifier identifier)
	{
		ArgumentNullException.ThrowIfNull(identifier);

		lock (this.synchronizationLock)
		{
			var status = this.statuses.GetValueOrDefault(identifier.Value) ??
				throw new InvalidOperationException("The Inventory Scan does not exist.");
			var summary = status.Summary;

			this.statuses[identifier.Value] = status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { TimeoutCount = summary.TimeoutCount + 1 }
			};
		}
	}

	public void RecordParticipant(
		InventoryScanStatusIdentifier identifier,
		byte port,
		ParameterValue agentType)
	{
		ArgumentNullException.ThrowIfNull(identifier);
		ArgumentNullException.ThrowIfNull(agentType);

		var participant = AgentTypeInventoryParticipant.FromParameterValue(port, agentType);

		lock (this.synchronizationLock)
		{
			var status = this.statuses.GetValueOrDefault(identifier.Value) ??
				throw new InvalidOperationException("The Inventory Scan does not exist.");
			var summary = status.Summary;

			this.statuses[identifier.Value] = status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Participants = [.. status.Participants, participant],
				Summary = summary with
				{
					DiscoveredParticipantCount = summary.DiscoveredParticipantCount + 1
				}
			};
		}
	}
}
