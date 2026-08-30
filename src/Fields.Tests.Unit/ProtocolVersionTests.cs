using AwesomeAssertions;

namespace Stensones.GD92.Fields.Tests.Unit;

public sealed class ProtocolVersionTests
{
	[Theory]
	[InlineData((byte)1)]
	[InlineData((byte)15)]
	public void Accepts_valid_protocol_versions(byte value)
	{
		ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(value)).Value.Should().Be(value);
	}

	[Theory]
	[InlineData((byte)0)]
	[InlineData((byte)16)]
	public void Rejects_invalid_protocol_versions(byte value)
	{
		Action createVersionNumber = () => ProtocolVersionNumber.FromValue(value);

		createVersionNumber.Should().Throw<ArgumentOutOfRangeException>();
	}
}
