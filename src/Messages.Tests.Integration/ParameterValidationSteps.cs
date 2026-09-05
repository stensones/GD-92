using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class ParameterValidationSteps
{
	private InvalidOperationException? decodingException;

	[Given(@"Parameter Contents with a partial Parameter Value byte")]
	public void GivenParameterContentsWithAPartialParameterValueByte()
	{
	}

	[When(@"Parameter Contents decoding is attempted")]
	public void WhenParameterContentsDecodingIsAttempted()
	{
		try
		{
			var buffer = new EncodedMessageBuffer([0x00, 0x00], bitPosition: 7);

			Parameter.FromEncodedMessageBuffer(ref buffer);
		}
		catch (InvalidOperationException exception)
		{
			this.decodingException = exception;
		}
	}

	[Then(@"the partial Parameter Value is rejected")]
	public void ThenThePartialParameterValueIsRejected()
	{
		this.decodingException.Should().NotBeNull();
	}

	[Given(@"Set Parameter Contents with a partial Parameter Value byte")]
	public void GivenSetParameterContentsWithAPartialParameterValueByte()
	{
	}

	[When(@"Set Parameter Contents decoding is attempted")]
	public void WhenSetParameterContentsDecodingIsAttempted()
	{
		try
		{
			var buffer = new EncodedMessageBuffer([0x00, 0x04, 0x0A], bitPosition: 7);

			SetParameter.FromEncodedMessageBuffer(ref buffer);
		}
		catch (InvalidOperationException exception)
		{
			this.decodingException = exception;
		}
	}

	[Then(@"the partial Set Parameter Value is rejected")]
	public void ThenThePartialSetParameterValueIsRejected()
	{
		this.decodingException.Should().NotBeNull();
	}
}
