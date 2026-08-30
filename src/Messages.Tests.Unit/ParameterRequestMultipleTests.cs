using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ParameterRequestMultipleTests
{
	[Fact]
	public void Serializes_a_single_Router_table_entry_request()
	{
		var request = ParameterRequestMultiple.FromFields(
			ParameterTable.Current,
			ParameterNumber.FromValue(13),
			ParameterEntryIndex.FromValue(1),
			ParameterEntryIndex.FromValue(1));

		request.Type.ToWireValue().Should().Equal(new byte[] { 0x3F });
		request.ToWireValue().Should().Equal(Convert.FromHexString("020D00010001"));
	}
}
