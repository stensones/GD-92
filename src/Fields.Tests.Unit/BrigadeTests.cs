using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class BrigadeTests
{
	[Fact]
	public void Preserves_an_unsigned_eight_bit_value()
	{
		var brigade = Brigade.FromValue(26);

		brigade.Value.Should().Be((byte)26);
	}
}
