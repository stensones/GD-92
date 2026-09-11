using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class TelephoneNumberTests
{
	[Fact]
	public void Serializes_a_counted_left_justified_telephone_number()
	{
		var telephoneNumber = TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("0123  "));

		telephoneNumber.ToWireValue().Should().Equal(Convert.FromHexString("06303132332020"));
	}

	[Fact]
	public void Rejects_a_telephone_number_containing_non_numeric_characters()
	{
		var createTelephoneNumber = () => TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("012-3"));

		createTelephoneNumber.Should().Throw<ArgumentOutOfRangeException>();
	}
}
