using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ProformaDefinitionTests
{
	[Fact]
	public void Serializes_the_format_type_and_table()
	{
		var definition = ProformaDefinition.FromFields(
			FormatType.FromValue(FormatTypeValue.TextTable),
			Table.FromValue(SevenBitAsciiString.FromValue("RESOURCE A1 READY")));

		definition.ToWireValue().Should().Equal(
			Convert.FromHexString("01115245534F55524345204131205245414459"));
	}
}
