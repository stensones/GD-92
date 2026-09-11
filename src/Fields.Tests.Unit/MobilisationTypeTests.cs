using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class MobilisationTypeTests
{
	[Theory]
	[InlineData(MobilisationTypeValue.PreAlert, 0x00)]
	[InlineData(MobilisationTypeValue.Incident, 0x01)]
	[InlineData(MobilisationTypeValue.NonIncident, 0x02)]
	[InlineData(MobilisationTypeValue.BatchMobiliseAddress, 0x03)]
	[InlineData(MobilisationTypeValue.Standby, 0x04)]
	[InlineData(MobilisationTypeValue.Demobilise, 0x05)]
	[InlineData(MobilisationTypeValue.Test, 0x06)]
	public void Serializes_each_defined_mobilisation_type(
		MobilisationTypeValue value,
		byte wireValue)
	{
		MobilisationType.FromValue(value).ToWireValue().Should().Equal(new byte[] { wireValue });
	}

	[Fact]
	public void Rejects_an_unassigned_mobilisation_type()
	{
		var decodeMobilisationType = () => DecodeMobilisationType(0x07);

		decodeMobilisationType.Should().Throw<ArgumentOutOfRangeException>();
	}

	private static MobilisationType DecodeMobilisationType(byte value)
	{
		var buffer = new EncodedMessageBuffer(new byte[] { value });

		return MobilisationType.FromEncodedMessageBuffer(ref buffer);
	}
}
