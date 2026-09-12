using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ResetTypeTests
{
	[Fact]
	public void Serializes_the_defined_software_reset_type()
	{
		ResetType.FromValue(ResetTypeValue.SoftwareReset)
			.ToWireValue()
			.Should()
			.Equal(new byte[] { 0x00 });
	}
}
