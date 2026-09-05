using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class EncodedMessageBufferSteps
{
	private InvalidOperationException? readException;

	[Given(@"an Encoded Message Buffer containing one octet")]
	public void GivenAnEncodedMessageBufferContainingOneOctet()
	{
	}

	[When(@"it attempts to read nine unsigned bits")]
	public void WhenItAttemptsToReadNineUnsignedBits()
	{
		try
		{
			var buffer = new EncodedMessageBuffer([0x00]);

			buffer.ReadUnsignedBits(9);
		}
		catch (InvalidOperationException exception)
		{
			this.readException = exception;
		}
	}

	[Then(@"the read beyond the encoded payload is rejected")]
	public void ThenTheReadBeyondTheEncodedPayloadIsRejected()
	{
		this.readException.Should().NotBeNull();
	}
}
