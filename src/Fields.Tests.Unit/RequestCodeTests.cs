using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class RequestCodeTests
{
	[Theory]
	[InlineData(RequestCodeValue.RequestToSpeak, 0x53)]
	[InlineData(RequestCodeValue.Emergency, 0x45)]
	[InlineData(RequestCodeValue.ConfidentialRequestToSpeak, 0x43)]
	public void Serializes_each_defined_request_code(RequestCodeValue value, byte expectedWireValue)
	{
		RequestCode.FromValue(value).ToWireValue().Should().Equal(new byte[] { expectedWireValue });
	}
}
