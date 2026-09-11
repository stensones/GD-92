using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class AlertGroupTests
{
	[Theory]
	[InlineData(AlertGroupValue.FirecallTeamA, "4641")]
	[InlineData(AlertGroupValue.FirecallTeamB, "4642")]
	[InlineData(AlertGroupValue.FirecallTeamC, "4643")]
	[InlineData(AlertGroupValue.FirecallTeamsAAndBAndC, "4644")]
	[InlineData(AlertGroupValue.FirecallTeamsAAndB, "4645")]
	[InlineData(AlertGroupValue.FirecallTeamsBAndC, "4646")]
	[InlineData(AlertGroupValue.FirecallTeamsAAndC, "4647")]
	public void Serializes_each_defined_firecall_team_combination(
		AlertGroupValue value,
		string expectedWireValue)
	{
		AlertGroup.FromValue(value).ToWireValue().Should().Equal(Convert.FromHexString(expectedWireValue));
	}
}
