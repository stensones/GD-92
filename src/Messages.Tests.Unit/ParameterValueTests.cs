using AwesomeAssertions;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ParameterValueTests
{
	[Fact]
	public void Preserves_a_returned_brigade_number_value()
	{
		var parameterValue = ParameterValue.FromWireValue(new byte[] { 0x1A });

		parameterValue.ToWireValue().Should().Equal(new byte[] { 0x1A });
	}
}
