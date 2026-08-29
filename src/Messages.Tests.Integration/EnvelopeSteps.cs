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
	private IGD92Message? message;
	private Envelope? envelope;

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
		this.message = Text.FromFields(
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
			this.message!);
	}

	[Then(@"its complete Envelope bytes are ""(.*)""")]
	public void ThenItsCompleteEnvelopeBytesAre(string expectedBytes)
	{
		this.envelope!.ToWireValue().Should().Equal(Convert.FromHexString(expectedBytes));
	}
}
