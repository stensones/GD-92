using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MoreValuesTests
{
	[Fact]
	public void Serializes_when_no_further_parameter_values_follow()
	{
		MoreValues.FromValue(ProtocolBoolean.False).ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Does_not_expose_a_primitive_factory()
	{
		typeof(MoreValues).GetMethod("FromValue", [typeof(byte)]).Should().BeNull();
	}

	[Fact]
	public void Decodes_the_affirmative_more_values_indicator()
	{
		var buffer = new EncodedMessageBuffer([0x01]);

		var moreValues = MoreValues.FromEncodedMessageBuffer(ref buffer);

		moreValues.Should().Be(MoreValues.Yes);
		buffer.RemainingBitCount.Should().Be(0);
	}
}
