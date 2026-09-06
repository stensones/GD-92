using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Participants;

public static class AgentTypeInventoryParticipant
{
	public static InventoryParticipant FromParameterValue(byte port, ParameterValue agentType)
	{
		ArgumentNullException.ThrowIfNull(agentType);

		var encodedAgentType = agentType.ToWireValue();
		if (encodedAgentType.Length != 1)
		{
			throw new ArgumentException("An Agent Type Parameter Value must contain one byte.", nameof(agentType));
		}

		return encodedAgentType[0] switch
		{
			0 => new InventoryParticipant(port, "mta", "ISDN (basic rate) (0)"),
			1 => new InventoryParticipant(port, "mta", "PSTN (1)"),
			2 => new InventoryParticipant(port, "mta", "Private wire (2)"),
			3 => new InventoryParticipant(port, "mta", "Radio (3)"),
			4 => new InventoryParticipant(port, "ua", "Printer (4)"),
			5 => new InventoryParticipant(port, "ua", "Alerter UA (Option 1) (5)"),
			6 => new InventoryParticipant(port, "ua", "Alerter UA (Option 2) (6)"),
			7 => new InventoryParticipant(port, "ua", "Paging UA (7)"),
			8 => new InventoryParticipant(port, "ua", "Peripheral UA (8)"),
			9 => new InventoryParticipant(port, "mta", "WAN MTA (9)"),
			10 => new InventoryParticipant(port, "mta", "LAN MTA (10)"),
			11 => new InventoryParticipant(port, "mta", "Asynch MTA (11)"),
			12 => new InventoryParticipant(port, "ua", "Network Management UA (12)"),
			13 => new InventoryParticipant(port, "ua", "MOBS UA (13)"),
			14 => new InventoryParticipant(port, "ua", "Standby MOBS UA (14)"),
			15 => new InventoryParticipant(port, "ua", "Resource UA (15)"),
			16 => new InventoryParticipant(port, "ua", "MDT UA (16)"),
			17 => new InventoryParticipant(port, "ua", "AVLS UA (17)"),
			18 => new InventoryParticipant(port, "mta", "Clarion (18)"),
			19 => new InventoryParticipant(port, "mta", "Paknet (19)"),
			20 => new InventoryParticipant(port, "mta", "RAM (20)"),
			21 => new InventoryParticipant(port, "mta", "Radio MTA (21)"),
			22 or 23 or 24 or 25 => new InventoryParticipant(
				port,
				"unclassified",
				$"Unclassified participant ({encodedAgentType[0]})"),
			_ => throw new ArgumentOutOfRangeException(
				nameof(agentType),
				encodedAgentType[0],
				"Agent Type must be in the GD-92 range 0 to 25.")
		};
	}
}
