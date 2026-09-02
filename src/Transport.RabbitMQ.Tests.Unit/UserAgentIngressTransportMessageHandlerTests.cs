using AwesomeAssertions;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class UserAgentIngressTransportMessageHandlerTests
{
	[Fact]
	public async Task Strictly_decodes_and_delivers_the_Parameter_response_Envelope_to_the_User_Agent_receiver()
	{
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));
		var receiver = new RecordingUserAgentIngressReceiver();
		var handler = new UserAgentIngressTransportMessageHandler();

		await handler.HandleAsync(
			new UserAgentIngressTransportMessage(envelope.ToWireValue()),
			receiver,
			CancellationToken.None);

		receiver.ReceivedEnvelope.Should().NotBeNull();
		receiver.ReceivedEnvelope.Should().NotBeSameAs(envelope);
		receiver.ReceivedEnvelope!.ToWireValue().Should().Equal(envelope.ToWireValue());
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class RecordingUserAgentIngressReceiver : IUserAgentIngressReceiver
	{
		public Envelope? ReceivedEnvelope { get; private set; }

		public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ReceivedEnvelope = envelope;
			return Task.CompletedTask;
		}
	}
}
