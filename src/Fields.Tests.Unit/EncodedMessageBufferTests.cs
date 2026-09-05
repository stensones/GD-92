using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class EncodedMessageBufferTests
{
	[Theory]
	[InlineData(-1)]
	[InlineData(9)]
	public void Rejects_a_starting_bit_position_outside_the_payload(int bitPosition)
	{
		Action createBuffer = () => InitializeBuffer(bitPosition);

		createBuffer.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(33)]
	public void Rejects_an_unsigned_read_width_outside_the_protocol_range(int bitCount)
	{
		Action read = () => ReadUnsignedBits(bitCount);

		read.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Rejects_an_unsigned_read_beyond_the_remaining_payload()
	{
		Action read = () => ReadUnsignedBits(9);

		read.Should().Throw<InvalidOperationException>()
			.WithMessage("*does not contain enough bits*");
	}

	private static void InitializeBuffer(int bitPosition)
	{
		_ = new EncodedMessageBuffer([0x00], bitPosition);
	}

	private static void ReadUnsignedBits(int bitCount)
	{
		var buffer = new EncodedMessageBuffer([0x00]);

		buffer.ReadUnsignedBits(bitCount);
	}
}
