using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ScalarParameterFieldTests
{
	[Fact]
	public void Round_trips_a_router_maximum_message_length()
	{
		var encoded = MaximumMessageLength.FromValue(0x1234).ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var maximumMessageLength = MaximumMessageLength.FromEncodedMessageBuffer(ref buffer);

		maximumMessageLength.Value.Should().Be(0x1234);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Theory]
	[InlineData("ManualAcknowledgementTimeout")]
	[InlineData("FrameTransmitCount")]
	[InlineData("FrameReceiveCount")]
	[InlineData("FrameTransmitFailureCount")]
	[InlineData("FrameReceiveFailureCount")]
	[InlineData("VerificationPeriod")]
	[InlineData("VerificationTimeout")]
	public void Serializes_named_sixteen_bit_parameter_values(string fieldName)
	{
		var wireValue = fieldName switch
		{
			"ManualAcknowledgementTimeout" => ManualAcknowledgementTimeout.FromValue(0x1234).ToWireValue(),
			"FrameTransmitCount" => FrameTransmitCount.FromValue(0x1234).ToWireValue(),
			"FrameReceiveCount" => FrameReceiveCount.FromValue(0x1234).ToWireValue(),
			"FrameTransmitFailureCount" => FrameTransmitFailureCount.FromValue(0x1234).ToWireValue(),
			"FrameReceiveFailureCount" => FrameReceiveFailureCount.FromValue(0x1234).ToWireValue(),
			"VerificationPeriod" => VerificationPeriod.FromValue(0x1234).ToWireValue(),
			"VerificationTimeout" => VerificationTimeout.FromValue(0x1234).ToWireValue(),
			_ => throw new ArgumentOutOfRangeException(nameof(fieldName))
		};

		wireValue.Should().Equal(Convert.FromHexString("1234"));
	}

	[Theory]
	[InlineData("FrameDuration")]
	[InlineData("FrameAcknowledgementTimeout")]
	[InlineData("RetriesAllowed")]
	public void Serializes_named_eight_bit_parameter_values(string fieldName)
	{
		var wireValue = fieldName switch
		{
			"FrameDuration" => FrameDuration.FromValue(0xA5).ToWireValue(),
			"FrameAcknowledgementTimeout" => FrameAcknowledgementTimeout.FromValue(0xA5).ToWireValue(),
			"RetriesAllowed" => RetriesAllowed.FromValue(0xA5).ToWireValue(),
			_ => throw new ArgumentOutOfRangeException(nameof(fieldName))
		};

		wireValue.Should().Equal(new byte[] { 0xA5 });
	}

	[Fact]
	public void Restricts_minimum_mta_priority_to_defined_message_priorities()
	{
		var decodeMinimumPriority = () =>
		{
			var buffer = new EncodedMessageBuffer(new byte[] { 0x00 });
			return MtaMinimumMessagePriority.FromEncodedMessageBuffer(ref buffer);
		};

		MtaMinimumMessagePriority.FromValue(MessagePriorityLevel.FromValue(1))
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x01 });
		decodeMinimumPriority.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Serializes_an_ascii_acknowledgement_character_and_rejects_non_ascii()
	{
		var createAcknowledgementCharacter = () => AcknowledgementCharacter.FromValue('\u0080');

		AcknowledgementCharacter.FromValue('A').ToWireValue().Should().Equal(new byte[] { 0x41 });
		createAcknowledgementCharacter.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Serializes_relocated_router_parameter_fields()
	{
		NoAcknowledgementTimeout.FromValue(Word8.FromValue(5))
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x05 });
		Retries.FromValue(Word8.FromValue(2))
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x02 });
	}
}
