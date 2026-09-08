using AwesomeAssertions;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class UserAgentIngressTransportMessageHandlerTests
{
	[Fact]
	public async Task Rejects_an_Envelope_with_trailing_bytes_using_the_transport_wide_error()
	{
		var wireValue = CreateEnvelope().ToWireValue().Append((byte)0).ToArray();
		var handler = new UserAgentIngressTransportMessageHandler();

		var handle = () => handler.HandleAsync(
			new UserAgentIngressTransportMessage(wireValue),
			new RecordingUserAgentIngressReceiver(),
			CancellationToken.None);

		var exception = await handle.Should().ThrowAsync<InvalidOperationException>();
		exception.Which.Message.Should().Be("The encoded ingress Envelope contains trailing bytes.");
	}

	[Fact]
	public async Task Strictly_decodes_and_delivers_the_Parameter_response_Envelope_to_the_User_Agent_receiver()
	{
		var envelope = CreateEnvelope();
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

	private static Envelope CreateEnvelope()
	{
		return Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
			Destinations.FromAddresses(Address(brigade: 26, node: 100, port: 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));
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
