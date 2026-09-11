using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ResourceStatusRequestTests
{
	[Fact]
	public void Serializes_the_requested_callsigns()
	{
		var request = ResourceStatusRequest.FromCallsigns(
			Callsign.FromValue(SevenBitAsciiString.FromValue("A1")));

		request.ToWireValue().Should().Equal(Convert.FromHexString("024131"));
	}

	[Fact]
	public void Serializes_a_zero_length_callsign_when_requesting_all_resources()
	{
		var request = ResourceStatusRequest.ForAllResources();

		request.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Decodes_the_end_terminated_requested_callsigns()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("024131"));

		var request = ResourceStatusRequest.FromEncodedMessageBuffer(ref buffer);

		request.Callsigns.Select(callsign => callsign.Value.Value).Should().Equal("A1");
		buffer.RemainingBitCount.Should().Be(0);
	}
}
