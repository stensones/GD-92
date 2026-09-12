using AwesomeAssertions;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class BrigadeMessageTests
{
	[Fact]
	public void Serializes_the_brigade_message_text()
	{
		var brigadeMessage = BrigadeMessage.FromFields(
			Stensones.GD92.Fields.Text.FromValue("PRINTER TEST"));

		brigadeMessage.ToWireValue().Should().Equal(
			Convert.FromHexString("000C5052494E5445522054455354"));
	}
}
