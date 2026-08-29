using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class Word8CreationSteps
{
	private byte value;
	private Word8? word8;

	[Given(@"the valid Word8 value (.*)")]
	public void GivenTheValidWord8Value(byte value)
	{
		this.value = value;
	}

	[When(@"a Word8 is created from the value")]
	public void WhenAWord8IsCreatedFromTheValue()
	{
		this.word8 = Word8.FromValue(this.value);
	}

	[Then(@"the Word8 serializes as ""(.*)""")]
	public void ThenTheWord8SerializesAs(string serializedValue)
	{
		this.word8!.ToWireValue().Should().Equal(Convert.FromHexString(serializedValue));
	}
}
