using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class DataBaseQueryTests
{
	[Fact]
	public void Serializes_the_query_type_and_text()
	{
		var query = DataBaseQuery.FromFields(
			QueryType.FromValue(0xA5),
			Stensones.GD92.Fields.Text.FromValue("SELECT STATUS"));

		query.ToWireValue().Should().Equal(
			Convert.FromHexString("A5000D53454C45435420535441545553"));
	}
}
