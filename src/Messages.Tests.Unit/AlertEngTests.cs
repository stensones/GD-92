using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class AlertEngTests
{
	[Fact]
	public void Serializes_the_alerter_engineering_command()
	{
		var alertEng = AlertEng.FromFields(
			AlerterEngineering.FromValue(AlerterEngineeringValue.LockSystemToTransmitterA));

		alertEng.ToWireValue().Should().Equal(Convert.FromHexString("41"));
	}
}
