using AwesomeAssertions;
using NodeManager.Router.Participants;
using Stensones.GD92.Fields;

namespace NodeManager.Tests.Unit.Router.Participants;

public sealed class InMemoryInventoryScanRegistryTests
{
	[Fact]
	public void Records_a_Parameter_Reason_Code_as_a_completed_probe()
	{
		var inventoryScans = new InMemoryInventoryScanRegistry();
		var identifier = inventoryScans.Start();

		inventoryScans.RecordNegativeAcknowledgement(
			identifier,
			ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax));

		var status = inventoryScans.Get(identifier)!;
		status.CompletedProbeCount.Should().Be(1);
		status.Summary.TimeoutCount.Should().Be(0);
		status.Summary.NegativeAcknowledgements.Should().ContainSingle()
			.Which.Should().Be(new KeyValuePair<string, int>("parameter:invalid_syntax", 1));
	}
}
