using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class MessageContentsCodecTests
{
	[Fact]
	public void Decodes_a_known_acknowledgement_contents_type()
	{
		var contents = MessageContentsCodec.Decode(
			MessageType.FromValue(GD92MessageType.Acknowledgement),
			[]);

		contents.Should().BeOfType<Acknowledgement>();
	}

	[Fact]
	public void Preserves_opaque_supplier_contents()
	{
		var wireValue = new byte[] { 0x01, 0x02, 0x03 };

		var contents = MessageContentsCodec.Decode(
			MessageType.FromValue(GD92MessageType.SupplierMessage),
			wireValue);
		wireValue[0] = 0xFF;

		contents.Should().BeOfType<UnsupportedMessageContents>();
		contents.ToWireValue().Should().Equal(0x01, 0x02, 0x03);
	}

	[Fact]
	public void Rejects_trailing_bytes_for_a_known_contents_type()
	{
		Action decode = () => MessageContentsCodec.Decode(
			MessageType.FromValue(GD92MessageType.Test),
			[0x00, 0x00]);

		decode.Should().Throw<InvalidOperationException>()
			.WithMessage("*length does not match*");
	}
}
