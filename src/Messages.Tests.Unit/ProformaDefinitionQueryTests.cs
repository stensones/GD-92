using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ProformaDefinitionQueryTests
{
	[Fact]
	public void Serializes_the_format_type()
	{
		var query = ProformaDefinitionQuery.FromFields(
			FormatType.FromValue(FormatTypeValue.TextTable));

		query.ToWireValue().Should().Equal(Convert.FromHexString("01"));
	}
}
