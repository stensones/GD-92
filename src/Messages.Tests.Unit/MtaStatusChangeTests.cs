using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class MtaStatusChangeTests
{
	[Fact]
	public void Serializes_the_mta_status()
	{
		var statusChange = MtaStatusChange.FromFields(
			MtaStatus.FromValue(MtaStatusValue.Online));

		statusChange.ToWireValue().Should().Equal(Convert.FromHexString("01"));
	}
}
