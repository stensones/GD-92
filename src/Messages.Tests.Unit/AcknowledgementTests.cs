using AwesomeAssertions;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class AcknowledgementTests
{
	[Fact]
	public void Has_type_50_and_no_contents_bytes()
	{
		var acknowledgement = Acknowledgement.Create();

		acknowledgement.Type.ToWireValue().Should().Equal(new byte[] { 0x32 });
		acknowledgement.ToWireValue().Should().BeEmpty();
	}
}
