using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PeripheralMapTests
{
	[Fact]
	public void Round_trips_an_input_peripheral_mapping()
	{
		var encoded = InputPeripheralMap.FromValues(
			PhysicalBit.FromValue(15),
			ActiveState.FromValue(ActiveStateValue.ActiveWhenContactsOpen),
			GenerateAlarm.FromValue(GenerateAlarmValue.GenerateAlarmOnBothAndEachRegenerationPeriod))
			.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var inputPeripheralMap = InputPeripheralMap.FromEncodedMessageBuffer(ref buffer);

		inputPeripheralMap.PhysicalBit.Value.Should().Be(15);
		inputPeripheralMap.ActiveState.Value.Should().Be(ActiveStateValue.ActiveWhenContactsOpen);
		inputPeripheralMap.GenerateAlarm.Value.Should().Be(GenerateAlarmValue.GenerateAlarmOnBothAndEachRegenerationPeriod);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Round_trips_an_output_peripheral_mapping()
	{
		var encoded = OutputPeripheralMap.FromValues(
			PhysicalBit.FromValue(3),
			ActiveState.FromValue(ActiveStateValue.ActiveWhenContactsClosed),
			PulseLength.FromValue(0x1234))
			.ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var outputPeripheralMap = OutputPeripheralMap.FromEncodedMessageBuffer(ref buffer);

		outputPeripheralMap.PhysicalBit.Value.Should().Be(3);
		outputPeripheralMap.ActiveState.Value.Should().Be(ActiveStateValue.ActiveWhenContactsClosed);
		outputPeripheralMap.PulseLength.Value.Should().Be(0x1234);
		buffer.RemainingBitCount.Should().Be(0);
	}
}
