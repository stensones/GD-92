using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class PeripheralStatusTests
{
	[Fact]
	public void Serializes_input_peripherals_before_output_peripherals()
	{
		var peripheralStatus = PeripheralStatus.FromFields(
			InputPeripherals.FromInputs(
				InputPeripheral.ManualAcknowledgementPressed,
				InputPeripheral.PaperLow),
			OutputPeripherals.FromOutputs(
				OutputPeripheral.StationSounders,
				OutputPeripheral.ApplianceIndicator1));

		peripheralStatus.ToWireValue().Should().Equal(Convert.FromHexString("00210101"));
	}

	[Fact]
	public void Decodes_input_peripherals_before_output_peripherals()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("00210101"));

		var peripheralStatus = PeripheralStatus.FromEncodedMessageBuffer(ref buffer);

		peripheralStatus.InputPeripherals.IsInputSet(InputPeripheral.ManualAcknowledgementPressed).Should().BeTrue();
		peripheralStatus.InputPeripherals.IsInputSet(InputPeripheral.PaperLow).Should().BeTrue();
		peripheralStatus.OutputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		peripheralStatus.OutputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
		buffer.RemainingBitCount.Should().Be(0);
	}
}
