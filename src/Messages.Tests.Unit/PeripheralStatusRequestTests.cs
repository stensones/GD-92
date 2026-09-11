using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class PeripheralStatusRequestTests
{
	[Fact]
	public void Serializes_an_empty_contents_payload()
	{
		var request = PeripheralStatusRequest.Create();

		request.ToWireValue().Should().BeEmpty();
	}

	[Fact]
	public void Rejects_non_empty_contents()
	{
		var decode = () => Decode(new byte[] { 0x00 });

		decode.Should().Throw<InvalidOperationException>();
	}

	private static PeripheralStatusRequest Decode(byte[] contents)
	{
		var buffer = new EncodedMessageBuffer(contents);

		return PeripheralStatusRequest.FromEncodedMessageBuffer(ref buffer);
	}
}
