using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class TimeAndDateTests
{
	[Fact]
	public void Serializes_a_fixed_length_submission_time_without_a_count()
	{
		var timeAndDate = TimeAndDate.FromValue(SevenBitAsciiString.FromValue("07SEP26154309"));

		timeAndDate.ToWireValue().Should().Equal(Convert.FromHexString("30375345503236313534333039"));
	}

	[Fact]
	public void Rejects_an_invalid_month_abbreviation()
	{
		var createTimeAndDate = () => TimeAndDate.FromValue(SevenBitAsciiString.FromValue("07XYZ26154309"));

		createTimeAndDate.Should().Throw<ArgumentOutOfRangeException>();
	}
}
