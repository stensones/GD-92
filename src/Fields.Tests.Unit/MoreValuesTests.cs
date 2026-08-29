using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MoreValuesTests
{
	[Fact]
	public void Serializes_when_no_further_parameter_values_follow()
	{
		MoreValues.No.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}
}
