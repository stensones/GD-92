using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MessageCodeFieldTests
{
	[Theory]
	[InlineData(PagerPriorityValue.Emergency, 0x45)]
	[InlineData(PagerPriorityValue.Priority, 0x50)]
	[InlineData(PagerPriorityValue.Routine, 0x52)]
	[InlineData(PagerPriorityValue.Administrative, 0x41)]
	public void Serializes_each_pager_priority(PagerPriorityValue value, byte expected)
	{
		PagerPriority.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Theory]
	[InlineData(PagerTypeValue.Alphanumeric, 0x41)]
	[InlineData(PagerTypeValue.Numeric, 0x4E)]
	[InlineData(PagerTypeValue.TonesOnly, 0x54)]
	public void Serializes_each_pager_type(PagerTypeValue value, byte expected)
	{
		PagerType.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Theory]
	[InlineData(StatusCodeValue.NoStatusData, 0x00)]
	[InlineData(StatusCodeValue.MobileToIncident, 0x01)]
	[InlineData(StatusCodeValue.Batched, 0x26)]
	public void Serializes_defined_resource_status_codes(StatusCodeValue value, byte expected)
	{
		StatusCode.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Theory]
	[InlineData(AlerterEngineeringValue.LockSystemToTransmitterA, 0x41)]
	[InlineData(AlerterEngineeringValue.RestoreAlternateMessageTransmitKeying, 0x5A)]
	[InlineData(AlerterEngineeringValue.UserDefinedParameterE, 0x45)]
	public void Serializes_defined_alerter_engineering_codes(AlerterEngineeringValue value, byte expected)
	{
		AlerterEngineering.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Fact]
	public void Serializes_a_defined_alerter_status_code()
	{
		AlerterStatus.FromValue(AlerterStatusValue.TotalTransmitterFailure)
			.ToWireValue()
			.Should()
			.Equal(Convert.FromHexString("787A"));
	}

	[Fact]
	public void Serializes_the_defined_no_avl_data_system_type()
	{
		AvlType.FromValue(AvlTypeValue.NoAvlDataSystemPresent)
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x00 });
	}

	[Theory]
	[InlineData(PrinterStatusValue.Offline, 0x00)]
	[InlineData(PrinterStatusValue.PaperOut, 0x01)]
	[InlineData(PrinterStatusValue.Online, 0x02)]
	public void Serializes_each_printer_status(PrinterStatusValue value, byte expected)
	{
		PrinterStatus.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Theory]
	[InlineData(MtaStatusValue.Idle, 0x00)]
	[InlineData(MtaStatusValue.Online, 0x01)]
	[InlineData(MtaStatusValue.OfflineUserInitiated, 0x02)]
	[InlineData(MtaStatusValue.OfflineFault, 0x03)]
	public void Serializes_each_mta_status(MtaStatusValue value, byte expected)
	{
		MtaStatus.FromValue(value).ToWireValue().Should().Equal(new byte[] { expected });
	}

	[Fact]
	public void Serializes_the_defined_text_table_format_type()
	{
		FormatType.FromValue(FormatTypeValue.TextTable)
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x01 });
	}

	[Fact]
	public void Serializes_a_pager_number_as_telephone_number_followed_by_pager_type()
	{
		PagerNumber.FromValues(
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123")),
			PagerType.FromValue(PagerTypeValue.Alphanumeric))
			.ToWireValue()
			.Should()
			.Equal(Convert.FromHexString("043031323341"));
	}

	[Fact]
	public void Serializes_an_alarm_reference_in_big_endian_order()
	{
		AlarmReference.FromValue(0x1234).ToWireValue().Should().Equal(Convert.FromHexString("1234"));
	}

	[Fact]
	public void Serializes_an_appliance_quantity()
	{
		ApplianceQuantity.FromValue(3).ToWireValue().Should().Equal(new byte[] { 0x03 });
	}

	[Fact]
	public void Rejects_riders_outside_the_defined_range()
	{
		var createRiders = () => Riders.FromValue(0);

		createRiders.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Rejects_a_number_of_appliance_types_above_twelve()
	{
		var createNumberTypes = () => NumberTypes.FromValue(13);

		createNumberTypes.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Preserves_an_opaque_test_type_value()
	{
		TestType.FromValue(0xA5).ToWireValue().Should().Equal(new byte[] { 0xA5 });
	}

	[Fact]
	public void Preserves_an_opaque_query_type_value()
	{
		QueryType.FromValue(0xA5).ToWireValue().Should().Equal(new byte[] { 0xA5 });
	}
}
