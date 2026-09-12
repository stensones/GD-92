using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ProtocolBooleanTests
{
	[Fact]
	public void Round_trips_a_true_protocol_boolean_without_exposing_its_encoded_byte()
	{
		var encoded = ProtocolBoolean.FromValue(true).ToWireValue();
		var buffer = new EncodedMessageBuffer(encoded);

		var protocolBoolean = ProtocolBoolean.FromEncodedMessageBuffer(ref buffer);

		protocolBoolean.Value.Should().BeTrue();
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Rejects_an_undefined_protocol_boolean_encoding()
	{
		var decodeProtocolBoolean = () =>
		{
			var buffer = new EncodedMessageBuffer(new byte[] { 0x02 });
			return ProtocolBoolean.FromEncodedMessageBuffer(ref buffer);
		};

		decodeProtocolBoolean.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Exposes_boolean_construction_and_value_without_a_byte_factory()
	{
		typeof(ProtocolBoolean).GetProperty(nameof(ProtocolBoolean.Value))!
			.PropertyType
			.Should()
			.Be(typeof(bool));
		typeof(ProtocolBoolean).GetMethod("FromValue", [typeof(byte)]).Should().BeNull();
	}
}
