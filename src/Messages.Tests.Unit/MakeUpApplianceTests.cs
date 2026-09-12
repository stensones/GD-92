using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class MakeUpApplianceTests
{
	[Fact]
	public void Serializes_appliance_type_and_quantity()
	{
		var appliance = MakeUpAppliance.FromFields(
			ApplianceType.FromValue(SevenBitAsciiString.FromValue("PMP")),
			ApplianceQuantity.FromValue(2));

		appliance.ToWireValue().Should().Equal(Convert.FromHexString("504D5002"));
	}
}
