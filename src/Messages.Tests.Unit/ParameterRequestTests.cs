using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ParameterRequestTests
{
	[Fact]
	public void Serializes_the_current_table_parameter_request()
	{
		var request = ParameterRequest.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(1));

		request.Type.ToWireValue().Should().Equal(new byte[] { 0x3D });
		request.ToWireValue().Should().Equal(new byte[] { 0x02, 0x01 });
	}
}
