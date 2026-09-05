using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class PasswordTests
{
	[Fact]
	public void Serializes_a_typed_password_value()
	{
		var value = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));

		var password = Password.FromValue(value);

		password.Value.Should().Be(value);
		password.ToWireValue().Should().Equal(Convert.FromHexString("0446495245"));
	}

	[Fact]
	public void Serializes_an_empty_password()
	{
		var password = Password.FromValue(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty)));

		password.ToWireValue().Should().Equal(new byte[] { 0x00 });
	}

	[Fact]
	public void Serializes_a_ten_character_7_bit_ASCII_password()
	{
		var password = Password.FromValue(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("0123456789")));

		password.ToWireValue().Should().Equal(Convert.FromHexString("0A30313233343536373839"));
	}

	[Fact]
	public void Rejects_a_password_value_longer_than_ten_characters()
	{
		var createPassword = () => PasswordValue.FromValue(
			SevenBitAsciiString.FromValue("01234567890"));

		createPassword.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Rejects_a_7_bit_ASCII_string_containing_non_ASCII_characters()
	{
		var createPassword = () => SevenBitAsciiString.FromValue("FIRÉ");

		createPassword.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Decodes_a_counted_7_bit_ASCII_password()
	{
		var buffer = new EncodedMessageBuffer(Convert.FromHexString("0446495245"));

		var password = Password.FromEncodedMessageBuffer(ref buffer);

		password.ToWireValue().Should().Equal(Convert.FromHexString("0446495245"));
		buffer.BitPosition.Should().Be(40);
	}
}
