using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class AlertCrewTests
{
	[Fact]
	public void Serializes_alert_group_manual_acknowledgement_request_and_output_peripherals()
	{
		var alertCrew = AlertCrew.FromFields(
			AlertGroup.FromValue(AlertGroupValue.FirecallTeamA),
			ManualAcknowledgementRequest.Required,
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));

		alertCrew.ToWireValue().Should().Equal(Convert.FromHexString("4641010101"));
	}

	[Fact]
	public void Decodes_alert_group_manual_acknowledgement_request_and_output_peripherals()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("4641010101"));

		var alertCrew = AlertCrew.FromEncodedMessageBuffer(ref buffer);

		alertCrew.AlertGroup.Value.Should().Be(AlertGroupValue.FirecallTeamA);
		alertCrew.ManualAcknowledgementRequest.Should().Be(ManualAcknowledgementRequest.Required);
		alertCrew.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		alertCrew.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
		buffer.RemainingBitCount.Should().Be(0);
	}
}
