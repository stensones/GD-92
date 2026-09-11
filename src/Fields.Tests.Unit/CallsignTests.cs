using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class CallsignTests
{
	[Fact]
	public void Serializes_a_counted_alphanumeric_callsign()
	{
		var callsign = Callsign.FromValue(SevenBitAsciiString.FromValue("F12"));

		callsign.ToWireValue().Should().Equal(Convert.FromHexString("03463132"));
	}

	[Fact]
	public void Rejects_a_callsign_containing_non_alphanumeric_characters()
	{
		var createCallsign = () => Callsign.FromValue(SevenBitAsciiString.FromValue("F-12"));

		createCallsign.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Serializes_a_counted_callsign_list()
	{
		var callsigns = CallsignList.FromValues(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			Callsign.FromValue(SevenBitAsciiString.FromValue("B2")));

		callsigns.ToWireValue().Should().Equal(Convert.FromHexString("02024131024232"));
	}

	[Fact]
	public void Decodes_every_callsign_in_a_callsign_list()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("02024131024232"));

		var callsigns = CallsignList.FromEncodedMessageBuffer(ref buffer);

		callsigns.Values.Select(callsign => callsign.Value.Value).Should().Equal("A1", "B2");
		buffer.BitPosition.Should().Be(56);
	}
}
