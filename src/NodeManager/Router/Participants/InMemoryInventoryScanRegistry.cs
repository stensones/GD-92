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

	public void RecordNegativeAcknowledgement(
		InventoryScanStatusIdentifier identifier,
		ReasonCode reasonCode)
	{
		ArgumentNullException.ThrowIfNull(identifier);
		ArgumentNullException.ThrowIfNull(reasonCode);

		lock (this.synchronizationLock)
		{
			var status = this.statuses.GetValueOrDefault(identifier.Value) ??
				throw new InvalidOperationException("The Inventory Scan does not exist.");
			var summary = status.Summary;
			var reasonCodeKey = FormatReasonCode(reasonCode);
			var negativeAcknowledgements = summary.NegativeAcknowledgements
				.ToDictionary(entry => entry.Key, entry => entry.Value);
			negativeAcknowledgements[reasonCodeKey] =
				negativeAcknowledgements.GetValueOrDefault(reasonCodeKey) + 1;

			this.statuses[identifier.Value] = status with
			{
				CompletedProbeCount = status.CompletedProbeCount + 1,
				Summary = summary with { NegativeAcknowledgements = negativeAcknowledgements }
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

	private static string FormatReasonCode(ReasonCode reasonCode)
	{
		return reasonCode switch
		{
			{ ParameterReasonCode: { } parameterReasonCode } =>
				$"parameter:{ToSnakeCase(parameterReasonCode.ToString())}",
			{ GeneralReasonCode: { } generalReasonCode } =>
				$"general:{ToSnakeCase(generalReasonCode.ToString())}",
			_ => throw new InvalidOperationException("The Reason Code set is not supported.")
		};
	}

	private static string ToSnakeCase(string value)
	{
		return string.Concat(value.Select((character, index) =>
			index > 0 && char.IsUpper(character)
				? $"_{char.ToLowerInvariant(character)}"
				: char.ToLowerInvariant(character).ToString()));
	}
}
