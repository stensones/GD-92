using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class InputPeripheralsTests
{
	[Theory]
	[InlineData(InputPeripheral.ManualAcknowledgementPressed, 0x00, 0x01)]
	[InlineData(InputPeripheral.PowerFailedBatteriesOn, 0x00, 0x02)]
	[InlineData(InputPeripheral.PowerFailedStandbyGeneratorOn, 0x00, 0x04)]
	[InlineData(InputPeripheral.RepeatLastMessagePressed, 0x00, 0x08)]
	[InlineData(InputPeripheral.BatteriesLow, 0x00, 0x10)]
	[InlineData(InputPeripheral.PaperLow, 0x00, 0x20)]
	public void Maps_each_defined_input_to_its_specification_bit(
		InputPeripheral input,
		byte highByte,
		byte lowByte)
	{
		InputPeripherals.FromInputs(input).ToWireValue()
			.Should()
			.Equal(new byte[] { highByte, lowByte });
	}
}
