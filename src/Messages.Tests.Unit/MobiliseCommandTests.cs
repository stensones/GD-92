using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class MobiliseCommandTests
{
	[Fact]
	public void Serializes_output_peripherals_then_the_manual_acknowledgement_request()
	{
		var mobiliseCommand = MobiliseCommand.FromFields(
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1),
			ManualAcknowledgementRequest.Required);

		mobiliseCommand.ToWireValue().Should().Equal(new byte[] { 0x01, 0x01, 0x01 });
	}

	[Fact]
	public void Creates_mobilise_command_contents_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x01, 0x01, 0x01 });

		var mobiliseCommand = MobiliseCommand.FromEncodedMessageBuffer(ref buffer);

		mobiliseCommand.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		mobiliseCommand.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
		mobiliseCommand.ManualAcknowledgementRequest.Should().Be(ManualAcknowledgementRequest.Required);
		buffer.BitPosition.Should().Be(24);
	}
}
