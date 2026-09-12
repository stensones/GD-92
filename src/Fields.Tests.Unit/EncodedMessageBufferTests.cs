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

	[Fact]
	public void Reads_an_aligned_fixed_length_byte_sequence()
	{
		var buffer = new EncodedMessageBuffer([0x0A, 0xBC, 0xDE]);

		buffer.ReadUnsignedBits(8).Should().Be(0x0A);

		var bytes = buffer.ReadBytes(2);

		bytes.ToArray().Should().Equal(0xBC, 0xDE);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Reads_the_remaining_aligned_bytes()
	{
		var buffer = new EncodedMessageBuffer([0x0A, 0xBC, 0xDE]);

		buffer.ReadUnsignedBits(8).Should().Be(0x0A);

		var bytes = buffer.ReadRemainingBytes();

		bytes.ToArray().Should().Equal(0xBC, 0xDE);
		buffer.RemainingBitCount.Should().Be(0);
	}

	[Fact]
	public void Rejects_an_unaligned_byte_read_without_advancing_the_cursor()
	{
		var buffer = new EncodedMessageBuffer([0x0A, 0xBC]);

		buffer.ReadUnsignedBits(1).Should().Be(0);

		InvalidOperationException? exception = null;

		try
		{
			buffer.ReadBytes(1);
		}
		catch (InvalidOperationException error)
		{
			exception = error;
		}

		exception.Should().NotBeNull();
		exception!.Message.Should().Match("*byte-aligned*");
		buffer.BitPosition.Should().Be(1);
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
