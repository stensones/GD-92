using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class Word8Tests
{
	[Theory]
	[InlineData((byte)0x00)]
	[InlineData((byte)0x2A)]
	[InlineData(byte.MaxValue)]
	public void Deserializes_and_serializes_a_single_byte_value(byte value)
	{
		var word8 = CreateWord8(value);

		word8.Value.Should().Be(value);
		word8.ToWireValue().Should().Equal(new byte[] { value });
	}

	[Fact]
	public void Values_with_the_same_encoded_value_are_equal()
	{
		var firstWord8 = CreateWord8(0x2A);
		var secondWord8 = CreateWord8(0x2A);

		secondWord8.Should().Be(firstWord8);
	}

	[Fact]
	public void Values_with_different_encoded_values_are_not_equal()
	{
		var firstWord8 = CreateWord8(0x2A);
		var secondWord8 = CreateWord8(0x2B);

		secondWord8.Should().NotBe(firstWord8);
	}

	[Fact]
	public void ToString_includes_the_word8_value()
	{
		var word8 = Word8.FromValue(42);

		word8.ToString().Should().Be("Word8 { Value = 42 }");
	}

	[Fact]
	public void FromValue_accepts_an_unsigned_eight_bit_value()
	{
		var factoryMethod = typeof(Word8).GetMethod(nameof(Word8.FromValue));

		factoryMethod.Should().NotBeNull();
		factoryMethod!.GetParameters().Should().ContainSingle()
			.Which.ParameterType.Should().Be(typeof(byte));
	}

	[Theory]
	[InlineData(byte.MinValue)]
	[InlineData(byte.MaxValue)]
	public void Creates_values_at_the_unsigned_eight_bit_boundaries(byte value)
	{
		var word8 = Word8.FromValue(value);

		word8.Value.Should().Be(value);
		word8.ToWireValue().Should().Equal(new byte[] { value });
	}

	private static Word8 CreateWord8(byte value)
	{
		var buffer = new EncodedMessageBuffer(new byte[] { value });

		return Word8.FromEncodedMessageBuffer(ref buffer);
	}
}
