using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class ParameterTests
{
	[Fact]
	public void Serializes_a_single_brigade_number_parameter_value()
	{
		var parameter = Parameter.FromFields(
			MoreValues.No,
			ParameterValue.FromWireValue(new byte[] { 0x1A }));

		parameter.Type.ToWireValue().Should().Equal(new byte[] { 0x3E });
		parameter.ToWireValue().Should().Equal(new byte[] { 0x00, 0x1A });
	}

	[Fact]
	public void Rejects_a_Parameter_Value_that_is_not_byte_aligned_during_decoding()
	{
		Action decodeParameter = () => DecodeParameter([0x00, 0x00], 7);

		decodeParameter.Should().Throw<InvalidOperationException>()
			.WithMessage("*byte-aligned*");
	}

	private static Parameter DecodeParameter(byte[] contents, int bitPosition)
	{
		var buffer = new EncodedMessageBuffer(contents, bitPosition);

		return Parameter.FromEncodedMessageBuffer(ref buffer);
	}
}
