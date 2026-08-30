using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ParameterEntryIndexTests
{
	[Fact]
	public void Serializes_an_entry_index_in_big_endian_order()
	{
		ParameterEntryIndex.FromValue(1).ToWireValue().Should().Equal(new byte[] { 0x00, 0x01 });
	}
}
