using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ParameterTableTests
{
	[Fact]
	public void Serializes_the_current_parameter_table()
	{
		ParameterTable.Current.ToWireValue().Should().Equal(new byte[] { 0x02 });
	}
}
