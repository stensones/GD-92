using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class AlertStatusTests
{
	[Fact]
	public void Serializes_the_alerter_status()
	{
		var alertStatus = AlertStatus.FromFields(
			AlerterStatus.FromValue(AlerterStatusValue.TotalTransmitterFailure));

		alertStatus.ToWireValue().Should().Equal(Convert.FromHexString("787A"));
	}
}
