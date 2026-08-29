using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ParameterNumberTests
{
	[Fact]
	public void Serializes_a_parameter_number()
	{
		ParameterNumber.FromValue(1).ToWireValue().Should().Equal(new byte[] { 0x01 });
	}
}
