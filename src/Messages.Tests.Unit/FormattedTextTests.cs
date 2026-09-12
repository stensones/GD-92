using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class FormattedTextTests
{
	[Fact]
	public void Serializes_the_format_type_and_table()
	{
		var formattedText = FormattedText.FromFields(
			FormatType.FromValue(FormatTypeValue.TextTable),
			Table.FromValue(SevenBitAsciiString.FromValue("RESOURCE A1 READY")));

		formattedText.ToWireValue().Should().Equal(
			Convert.FromHexString("01115245534F55524345204131205245414459"));
	}
}
