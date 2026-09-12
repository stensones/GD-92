using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class AreaPageMessageTests
{
	[Fact]
	public void Serializes_pager_priority_number_and_text()
	{
		var areaPageMessage = AreaPageMessage.FromFields(
			PagerPriority.FromValue(PagerPriorityValue.Routine),
			PagerNumber.FromValues(
				TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123")),
				PagerType.FromValue(PagerTypeValue.Alphanumeric)),
			PagerText.FromValue(SevenBitAsciiString.FromValue("HELLO")));

		areaPageMessage.ToWireValue().Should().Equal(Convert.FromHexString("520430313233410548454C4C4F"));
	}
}
