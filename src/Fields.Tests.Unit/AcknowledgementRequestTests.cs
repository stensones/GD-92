using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class AcknowledgementRequestTests
{
	[Fact]
	public void Represents_a_requested_acknowledgement()
	{
		AcknowledgementRequest.Requested.IsRequested.Should().BeTrue();
		AcknowledgementRequest.NotRequested.IsRequested.Should().BeFalse();
	}
}
