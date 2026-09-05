using AwesomeAssertions;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Unit;

public sealed class AcknowledgementTests
{
	[Fact]
	public void Has_type_50_and_no_contents_bytes()
	{
		var acknowledgement = Acknowledgement.Create();

		acknowledgement.Type.ToWireValue().Should().Equal(new byte[] { 0x32 });
		acknowledgement.ToWireValue().Should().BeEmpty();
	}

	[Fact]
	public void Rejects_non_empty_contents_during_decoding()
	{
		Action decodeAcknowledgement = () => DecodeAcknowledgement([0x00]);

		decodeAcknowledgement.Should().Throw<InvalidOperationException>()
			.WithMessage("*must be empty*");
	}

	private static Acknowledgement DecodeAcknowledgement(byte[] contents)
	{
		var buffer = new EncodedMessageBuffer(contents);

		return Acknowledgement.FromEncodedMessageBuffer(ref buffer);
	}
}
