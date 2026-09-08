using AwesomeAssertions;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Integration;

[Binding]
public sealed class IngressEnvelopeTransportSteps
{
	private Func<Task>? decode;

	[Given(@"Local Participant Ingress receives an encoded Envelope with a trailing byte")]
	public void GivenLocalParticipantIngressReceivesAnEncodedEnvelopeWithATrailingByte()
	{
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 24)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([25])));
		var wireValue = envelope.ToWireValue().Append((byte)0).ToArray();
		var handler = new LocalParticipantIngressTransportMessageHandler();

		this.decode = () => handler.HandleAsync(
			new LocalParticipantIngressTransportMessage(wireValue),
			new ReceivingLocalParticipant(),
			CancellationToken.None);
	}

	[When(@"it decodes the ingress transport message")]
	public void WhenItDecodesTheIngressTransportMessage()
	{
	}

	[Then(@"Ingress Envelope Transport rejects the trailing byte")]
	public async Task ThenIngressEnvelopeTransportRejectsTheTrailingByte()
	{
		var exception = await this.decode.Should().ThrowAsync<InvalidOperationException>();
		exception.Which.Message.Should().Be("The encoded ingress Envelope contains trailing bytes.");
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class ReceivingLocalParticipant : ILocalParticipantIngressReceiver
	{
		public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
