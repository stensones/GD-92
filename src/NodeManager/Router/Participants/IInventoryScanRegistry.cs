using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Router.Participants;

public interface IInventoryScanRegistry
{
	InventoryScanStatusIdentifier Start();
	InventoryScanStatus? Get(InventoryScanStatusIdentifier identifier);
	void RecordParticipant(
		InventoryScanStatusIdentifier identifier,
		byte port,
		ParameterValue agentType);
	void RecordTimeout(InventoryScanStatusIdentifier identifier);
	void RecordDeliveryFailure(InventoryScanStatusIdentifier identifier);
	void RecordNegativeAcknowledgement(
		InventoryScanStatusIdentifier identifier,
		ReasonCode reasonCode);
}
