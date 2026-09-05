using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ReasonCodeTests
{
	[Fact]
	public void Serializes_the_general_invalid_message_reason_code()
	{
		var reasonCode = ReasonCode.FromGeneralReasonCode(GeneralReasonCode.InvalidMessage);

		reasonCode.ToWireValue().Should().Equal(new byte[] { 0x01, 0x03 });
	}

	[Fact]
	public void Decodes_the_general_invalid_message_reason_code()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x01, 0x03 });

		var reasonCode = ReasonCode.FromEncodedMessageBuffer(ref buffer);

		reasonCode.GeneralReasonCode.Should().Be(GeneralReasonCode.InvalidMessage);
		buffer.BitPosition.Should().Be(16);
	}

	[Fact]
	public void Rejects_an_unsupported_encoded_reason_code_set()
	{
		var decodeReasonCode = () => DecodeReasonCode(0x02, 0x01);

		decodeReasonCode.Should().Throw<NotSupportedException>();
	}

	[Fact]
	public void Rejects_an_undefined_encoded_general_reason_code()
	{
		var decodeReasonCode = () => DecodeReasonCode(0x01, 0x00);

		decodeReasonCode.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Decodes_a_parameter_invalid_table_reason_code()
	{
		var buffer = new EncodedMessageBuffer([0x04, 0x05]);

		var reasonCode = ReasonCode.FromEncodedMessageBuffer(ref buffer);

		reasonCode.ParameterReasonCode.Should().Be(ParameterReasonCode.InvalidTable);
		reasonCode.GeneralReasonCode.Should().BeNull();
		reasonCode.ToWireValue().Should().Equal([0x04, 0x05]);
	}

	[Fact]
	public void Rejects_an_undefined_encoded_parameter_reason_code()
	{
		Action decodeReasonCode = () => DecodeReasonCode(0x04, 0x00);

		decodeReasonCode.Should().Throw<ArgumentOutOfRangeException>();
	}

	private static void DecodeReasonCode(byte reasonCodeSet, byte reasonCodeValue)
	{
		var buffer = new EncodedMessageBuffer(new byte[] { reasonCodeSet, reasonCodeValue });

		ReasonCode.FromEncodedMessageBuffer(ref buffer);
	}
}
