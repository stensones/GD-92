using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages.Tests.Integration;

[Binding]
public sealed class EnvelopeSteps
{
	private CommunicationsAddress? source;
	private Destinations? destinations;
	private ProtocolAndPriority? protocolAndPriority;
	private AcknowledgementAndSequence? acknowledgementAndSequence;
	private IGD92MessageContents? contents;
	private Envelope? envelope;
	private byte[]? encodedEnvelope;
	private InvalidOperationException? decodingException;

	[Given(@"an Envelope source and destination of Brigade (.*), Node (.*), and Port (.*)")]
	public void GivenAnEnvelopeSourceAndDestination(byte brigade, ushort node, byte port)
	{
		var address = CommunicationsAddress.FromValues(
			Brigade.FromValue(brigade),
			Node.FromValue(node),
			Port.FromValue(port));

		this.source = address;
		this.destinations = Destinations.FromAddresses(address);
	}

	[Given(@"an Envelope priority of (.*) and protocol version of (.*)")]
	public void GivenAnEnvelopePriorityAndProtocolVersion(byte priority, byte protocolVersion)
	{
		this.protocolAndPriority = ProtocolAndPriority.FromValues(
			MessagePriority.FromValue(priority),
			ProtocolVersion.FromValue(protocolVersion));
	}

	[Given(@"an Envelope sequence number of (.*) requesting acknowledgement")]
	public void GivenAnEnvelopeSequenceNumberRequestingAcknowledgement(ushort sequenceNumber)
	{
		this.acknowledgementAndSequence = AcknowledgementAndSequence.FromValues(
			SequenceNumber.FromValue(sequenceNumber),
			AcknowledgementRequest.Requested);
	}

	[Given(@"a single-block Text message containing ""(.*)""")]
	public void GivenASingleBlockTextMessageContaining(string text)
	{
		this.contents = Text.FromFields(
			Block.FromValue(1),
			OfBlocks.FromValue(1),
			Stensones.GD92.Fields.Text.FromValue(text));
	}

	[When(@"the Envelope is created")]
	public void WhenTheEnvelopeIsCreated()
	{
		this.envelope = Envelope.FromValues(
			this.source!,
			this.destinations!,
			this.protocolAndPriority!,
			this.acknowledgementAndSequence!,
			this.contents!);
	}

	[Then(@"its complete Envelope bytes are ""(.*)""")]
	public void ThenItsCompleteEnvelopeBytesAre(string expectedBytes)
	{
		this.envelope!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}

	[Given(@"encoded Envelope bytes ""(.*)""")]
	public void GivenEncodedEnvelopeBytes(string encodedEnvelope)
	{
		this.encodedEnvelope = Convert.FromHexString(encodedEnvelope);
	}

	[When(@"the Envelope is decoded")]
	public void WhenTheEnvelopeIsDecoded()
	{
		var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);

		this.envelope = Envelope.FromEncodedMessageBuffer(ref buffer);
	}

	[Then(@"its decoded source is Brigade (.*), Node (.*), Port (.*)")]
	public void ThenItsDecodedSourceIs(byte brigade, ushort node, byte port)
	{
		this.envelope!.Source.Should().Be(CommunicationsAddress.FromValues(
			Brigade.FromValue(brigade),
			Node.FromValue(node),
			Port.FromValue(port)));
	}

	[Then(@"it has (.*) decoded destination")]
	public void ThenItHasDecodedDestination(byte expectedCount)
	{
		this.envelope!.Destinations.Count.Value.Should().Be(expectedCount);
	}

	[Then(@"its decoded Text Message Contents are block (.*) of (.*) containing ""(.*)""")]
	public void ThenItsDecodedTextMessageContentsAre(byte block, byte ofBlocks, string text)
	{
		var contents = this.envelope!.Contents.Should().BeOfType<Text>().Which;

		contents.Block.Value.Should().Be(block);
		contents.OfBlocks.Value.Should().Be(ofBlocks);
		contents.MessageText.Value.Should().Be(text);
	}

	[When(@"decoding the Envelope is attempted")]
	public void WhenDecodingTheEnvelopeIsAttempted()
	{
		try
		{
			var buffer = new EncodedMessageBuffer(this.encodedEnvelope!);

			this.envelope = Envelope.FromEncodedMessageBuffer(ref buffer);
		}
		catch (InvalidOperationException exception)
		{
			this.decodingException = exception;
		}
	}

	[Then(@"the Block Check Character mismatch is rejected")]
	public void ThenTheBlockCheckCharacterMismatchIsRejected()
	{
		this.decodingException.Should().NotBeNull();
	}
}
