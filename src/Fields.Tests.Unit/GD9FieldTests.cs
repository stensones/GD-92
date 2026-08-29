using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class GD9FieldTests
{
	[Fact]
	public void All_protocol_fields_implement_the_marker_interface()
	{
		var fieldTypes = new[]
		{
			typeof(Word8),
			typeof(Block),
			typeof(OfBlocks),
			typeof(Text),
			typeof(CommunicationsAddress),
			typeof(ProtocolAndPriority),
			typeof(AcknowledgementAndSequence),
		};

		foreach (var fieldType in fieldTypes)
		{
			typeof(IGD9Field).IsAssignableFrom(fieldType).Should().BeTrue();
		}
	}
}
