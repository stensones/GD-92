using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class PageOfficerTests
{
	[Fact]
	public void Serializes_pager_priority_number_and_text()
	{
		var pageOfficer = PageOfficer.FromFields(
			PagerPriority.FromValue(PagerPriorityValue.Emergency),
			PagerNumber.FromValues(
				TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123")),
				PagerType.FromValue(PagerTypeValue.Alphanumeric)),
			PagerText.FromValue(SevenBitAsciiString.FromValue("AAAAA")));

		pageOfficer.ToWireValue().Should().Equal(Convert.FromHexString("45043031323341031B4105"));
	}
}
