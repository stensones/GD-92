using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class OutputPeripheralsTests
{
	[Theory]
	[InlineData(OutputPeripheral.StationSounders, 0x00, 0x01)]
	[InlineData(OutputPeripheral.StationLights, 0x00, 0x02)]
	[InlineData(OutputPeripheral.StationDoors, 0x00, 0x20)]
	[InlineData(OutputPeripheral.StandbySounder, 0x00, 0x40)]
	[InlineData(OutputPeripheral.ApplianceIndicator1, 0x01, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator2, 0x02, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator3, 0x04, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator4, 0x08, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator5, 0x10, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator6, 0x20, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator7, 0x40, 0x00)]
	[InlineData(OutputPeripheral.ApplianceIndicator8, 0x80, 0x00)]
	public void Maps_each_defined_output_to_its_specification_bit(
		OutputPeripheral output,
		byte highByte,
		byte lowByte)
	{
		OutputPeripherals.FromOutputs(output).ToWireValue()
			.Should()
			.Equal(new byte[] { highByte, lowByte });
	}

	[Fact]
	public void Serializes_named_outputs_in_big_endian_order()
	{
		OutputPeripherals.FromOutputs(
			OutputPeripheral.StationSounders,
			OutputPeripheral.StandbySounder,
			OutputPeripheral.ApplianceIndicator1)
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x01, 0x41 });
	}

	[Fact]
	public void Identifies_whether_a_specified_output_is_set()
	{
		var outputPeripherals = OutputPeripherals.FromOutputs(
			OutputPeripheral.StationSounders,
			OutputPeripheral.ApplianceIndicator5);

		outputPeripherals.IsOutputSet(OutputPeripheral.StationSounders).Should().BeTrue();
		outputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator5).Should().BeTrue();
		outputPeripherals.IsOutputSet(OutputPeripheral.StationLights).Should().BeFalse();
	}

	[Fact]
	public void Creates_output_peripherals_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x81, 0x02 });

		var outputPeripherals = OutputPeripherals.FromEncodedMessageBuffer(ref buffer);

		outputPeripherals.IsOutputSet(OutputPeripheral.StationLights).Should().BeTrue();
		outputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator1).Should().BeTrue();
		outputPeripherals.IsOutputSet(OutputPeripheral.ApplianceIndicator8).Should().BeTrue();
		buffer.BitPosition.Should().Be(16);
	}

	[Fact]
	public void Preserves_reserved_bits_when_decoding()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x00, 0x04 });

		var outputPeripherals = OutputPeripherals.FromEncodedMessageBuffer(ref buffer);

		outputPeripherals.ReservedBits.Should().Be(0x0004);
		outputPeripherals.ToWireValue().Should().Equal(new byte[] { 0x00, 0x04 });
	}

	[Fact]
	public void Rejects_undefined_outputs()
	{
		var undefinedOutput = (OutputPeripheral)0x0004;

		var createOutputPeripherals = () => OutputPeripherals.FromOutputs(undefinedOutput);
		var checkOutput = () => OutputPeripherals.FromOutputs().IsOutputSet(undefinedOutput);

		createOutputPeripherals.Should().Throw<ArgumentOutOfRangeException>();
		checkOutput.Should().Throw<ArgumentOutOfRangeException>();
	}
}
