using AwesomeAssertions;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Router.Tests.Unit;

public sealed class RouterIngressReceiverTests
{
	[Fact]
	public async Task Retains_the_Router_response_for_a_received_Envelope()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var receiver = new RouterIngressReceiver(new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))));

		await receiver.ReceiveAsync(CreateEnvelope(routerAddress), CancellationToken.None);

		receiver.Response.Should().NotBeNull();
	}

	private static Envelope CreateEnvelope(CommunicationsAddress routerAddress)
	{
		var source = CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(25)));

		return Envelope.FromValues(
			source,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1)));
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
