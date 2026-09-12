using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ResetTypeTests
{
	[Theory]
	[InlineData(ResetTypeValue.SoftwareReset, 0x00)]
	[InlineData(ResetTypeValue.HardwareReset, 0x01)]
	[InlineData(ResetTypeValue.HardwareResetAndReloadParametersFromPermanentTables, 0x02)]
	public void Serializes_each_defined_reset_type(ResetTypeValue value, byte expectedWireValue)
	{
		ResetType.FromValue(value)
			.ToWireValue()
			.Should()
			.Equal(new byte[] { expectedWireValue });
	}
}
