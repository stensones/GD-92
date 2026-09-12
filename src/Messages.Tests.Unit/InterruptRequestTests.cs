using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class InterruptRequestTests
{
	[Fact]
	public void Serializes_callsign_request_code_and_text()
	{
		var interruptRequest = InterruptRequest.FromFields(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")),
			RequestCode.FromValue(RequestCodeValue.Emergency),
			Stensones.GD92.Fields.Text.FromValue("MAYDAY"));

		interruptRequest.ToWireValue().Should().Equal(Convert.FromHexString("0241314500064D4159444159"));
	}

	[Fact]
	public void Decodes_callsign_request_code_and_text()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("0241314500064D4159444159"));

		var interruptRequest = InterruptRequest.FromEncodedMessageBuffer(ref buffer);

		interruptRequest.Callsign.Value.Value.Should().Be("A1");
		interruptRequest.RequestCode.Value.Should().Be(RequestCodeValue.Emergency);
		interruptRequest.Text.Value.Should().Be("MAYDAY");
		buffer.RemainingBitCount.Should().Be(0);
	}
}
