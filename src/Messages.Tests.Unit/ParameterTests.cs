using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ParameterTests
{
	[Fact]
	public void Serializes_a_single_brigade_number_parameter_value()
	{
		var parameter = Parameter.FromFields(
			MoreValues.No,
			ParameterValue.FromWireValue(new byte[] { 0x1A }));

		parameter.Type.ToWireValue().Should().Equal(new byte[] { 0x3E });
		parameter.ToWireValue().Should().Equal(new byte[] { 0x00, 0x1A });
	}
}
