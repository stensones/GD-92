using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace LANMTA.Persistence;

public sealed class LanMtaParameterBootstrapConfiguration
{
	private LanMtaParameterBootstrapConfiguration(
		Word8 portNumber,
		AgentType agentType)
	{
		this.PortNumber = portNumber;
		this.AgentType = agentType;
	}

	public Word8 PortNumber { get; }
	public AgentType AgentType { get; }

	public static LanMtaParameterBootstrapConfiguration FromAddress(
		CommunicationsAddress address)
	{
		ArgumentNullException.ThrowIfNull(address);

		return new LanMtaParameterBootstrapConfiguration(
			Word8.FromValue(address.Port.Value),
			AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent));
	}
}
