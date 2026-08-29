using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class SequenceNumberTests
{
	[Theory]
	[InlineData((ushort)0)]
	[InlineData((ushort)32767)]
	public void Accepts_valid_sequence_numbers(ushort value)
	{
		SequenceNumber.FromValue(value).Value.Should().Be(value);
	}

	[Fact]
	public void Rejects_values_above_the_unsigned_fifteen_bit_range()
	{
		Action createSequenceNumber = () => SequenceNumber.FromValue(32768);

		createSequenceNumber.Should().Throw<ArgumentOutOfRangeException>();
	}
}
