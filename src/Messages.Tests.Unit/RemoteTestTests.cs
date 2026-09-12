using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class RemoteTestTests
{
	[Fact]
	public void Serializes_the_opaque_test_type()
	{
		var test = Test.FromFields(TestType.FromValue(0xA5));

		test.ToWireValue().Should().Equal(Convert.FromHexString("A5"));
	}
}
