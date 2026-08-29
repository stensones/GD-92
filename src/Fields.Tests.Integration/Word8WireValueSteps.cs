using AwesomeAssertions;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class Word8WireValueSteps
{
	private byte[]? serializedData;
	private int initialBitPosition;
	private int byteOffset;
	private int bitOffsetInByte;
	private Word8? originalWord8;
	private Word8? newWord8;

	[Given(@"encoded frame payload ""(.*)""")]
	public void GivenEncodedFramePayload(string payload)
	{
		this.serializedData = Convert.FromHexString(payload);
	}

	[Given(@"the encoded message buffer is positioned at bit (.*)")]
	public void GivenTheEncodedMessageBufferIsPositionedAtBit(int bitPosition)
	{
		this.initialBitPosition = bitPosition;
	}

	[When(@"a Word8 is created from the encoded message buffer")]
	public void WhenAWord8IsCreatedFromTheEncodedMessageBuffer()
	{
		var buffer = new EncodedMessageBuffer(this.serializedData!, this.initialBitPosition);

		this.originalWord8 = Word8.FromEncodedMessageBuffer(ref buffer);
		this.byteOffset = buffer.ByteOffset;
		this.bitOffsetInByte = buffer.BitOffsetInByte;
	}

	[When(@"the Word8 is serialized")]
	public void WhenTheWord8IsSerialized()
	{
		this.serializedData = this.originalWord8!.ToWireValue();
	}

	[Then(@"the serialized Word8 data is ""(.*)""")]
	public void ThenTheSerializedWord8DataIs(string expectedSerializedData)
	{
		this.serializedData.Should().Equal(Convert.FromHexString(expectedSerializedData));
	}

	[When(@"a new Word8 is created from the serialized data")]
	public void WhenANewWord8IsCreatedFromTheSerializedData()
	{
		var newBuffer = new EncodedMessageBuffer(this.serializedData!, 0);
		this.newWord8 = Word8.FromEncodedMessageBuffer(ref newBuffer);
	}

	[Then(@"the new Word8 is value identical to the original Word8")]
	public void ThenTheNewWord8IsValueIdenticalToTheOriginalWord8()
	{
		this.newWord8.Should().Be(this.originalWord8);
	}

	[Then(@"the encoded message buffer is positioned at byte (.*) and bit (.*)")]
	public void ThenTheEncodedMessageBufferIsPositionedAtByteAndBit(int byteOffset, int bitOffsetInByte)
	{
		this.byteOffset.Should().Be(byteOffset);
		this.bitOffsetInByte.Should().Be(bitOffsetInByte);
	}
}
