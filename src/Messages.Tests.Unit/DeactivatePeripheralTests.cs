using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class DeactivatePeripheralTests
{
	[Fact]
	public void Serializes_the_selected_output_peripherals()
	{
		var deactivatePeripheral = DeactivatePeripheral.FromFields(
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));

		deactivatePeripheral.ToWireValue().Should().Equal(new byte[] { 0x01, 0x01 });
	}

	[Fact]
	public void Decodes_the_selected_output_peripherals()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x01, 0x01 });

		var deactivatePeripheral = DeactivatePeripheral.FromEncodedMessageBuffer(ref buffer);

		deactivatePeripheral.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		deactivatePeripheral.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
		buffer.BitPosition.Should().Be(16);
	}
}
