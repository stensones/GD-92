using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class CountAndLengthTests
{
	[Fact]
	public void Serializes_message_length_then_destination_count()
	{
		var field = CountAndLength.FromValues(
			MessageLength.FromValue(MessageByteLength.FromValue(8)),
			DestinationCount.FromValue(DestinationAddressCount.FromValue(1)));

		field.ToWireValue().Should().Equal(new byte[] { 0x02, 0x01 });
	}

	[Fact]
	public void Creates_count_and_length_from_an_encoded_message_buffer()
	{
		var buffer = new EncodedMessageBuffer(new byte[] { 0x02, 0x01 });

		CountAndLength.FromEncodedMessageBuffer(ref buffer).ToWireValue().Should().Equal(new byte[] { 0x02, 0x01 });
		buffer.BitPosition.Should().Be(16);
	}
}
