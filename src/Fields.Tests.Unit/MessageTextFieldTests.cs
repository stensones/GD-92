using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MessageTextFieldTests
{
	[Theory]
	[InlineData("AlarmType", "FIRE", "0446495245")]
	[InlineData("CallAgency", "ACME", "0441434D45")]
	[InlineData("AlarmSerial", "ABC123", "06414243313233")]
	[InlineData("AvlData", "GPS", "03475053")]
	[InlineData("OfficerInCharge", "SMITH", "05534D495448")]
	public void Serializes_bounded_plain_ascii_fields(string fieldName, string value, string expectedWireValue)
	{
		var wireValue = fieldName switch
		{
			"AlarmType" => AlarmType.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"CallAgency" => CallAgency.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"AlarmSerial" => AlarmSerial.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"AvlData" => AvlData.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"OfficerInCharge" => OfficerInCharge.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			_ => throw new ArgumentOutOfRangeException(nameof(fieldName))
		};

		wireValue.Should().Equal(Convert.FromHexString(expectedWireValue));
	}

	[Theory]
	[InlineData("PagerText", "AAAAA", "031B4105")]
	[InlineData("Remarks", "AAAAA", "031B4105")]
	[InlineData("Update", "AAAAA", "031B4105")]
	[InlineData("Table", "AAAAA", "031B4105")]
	public void Serializes_bounded_compressed_ascii_fields(string fieldName, string value, string expectedWireValue)
	{
		var wireValue = fieldName switch
		{
			"PagerText" => PagerText.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"Remarks" => Remarks.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"Update" => Update.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			"Table" => Table.FromValue(SevenBitAsciiString.FromValue(value)).ToWireValue(),
			_ => throw new ArgumentOutOfRangeException(nameof(fieldName))
		};

		wireValue.Should().Equal(Convert.FromHexString(expectedWireValue));
	}

	[Fact]
	public void Rejects_an_alarm_type_longer_than_ten_characters()
	{
		var createAlarmType = () => AlarmType.FromValue(SevenBitAsciiString.FromValue("ABCDEFGHIJK"));

		createAlarmType.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Decodes_a_compressed_pager_text_field()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("031B4105"));

		var pagerText = PagerText.FromEncodedMessageBuffer(ref buffer);

		pagerText.Value.Value.Should().Be("AAAAA");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Encodes_an_appliance_type_as_a_space_padded_three_byte_identifier()
	{
		ApplianceType.FromValue(SevenBitAsciiString.FromValue("HP"))
			.ToWireValue()
			.Should()
			.Equal(Convert.FromHexString("485020"));
	}

	[Fact]
	public void Decodes_a_space_padded_appliance_type()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("485020"));

		var applianceType = ApplianceType.FromEncodedMessageBuffer(ref buffer);

		applianceType.Value.Value.Should().Be("HP");
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Rejects_a_stop_code_that_is_not_exactly_five_alphanumeric_characters()
	{
		var createStopCode = () => StopCode.FromValue(SevenBitAsciiString.FromValue("AB12"));

		createStopCode.Should().Throw<ArgumentOutOfRangeException>();
	}
}
