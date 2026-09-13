using AwesomeAssertions;
using NodeManager.Persistence;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit;

public sealed class NodeManagerParticipantIngressReceiverTests
{
	[Theory]
	[InlineData(1, 25)]
	[InlineData(2, 12)]
	public async Task Returns_the_requested_local_current_identity_parameter(
		byte parameterNumber,
		byte expectedValue)
	{
		var nodeManager = Address(26, 100, 25);
		var projectionSource = new NodeManagerCurrentParameterProjectionSource();
		projectionSource.Publish(NodeManagerCurrentParameterProjection.FromNonVolatileParameters(
			ParameterValue.FromWireValue([25]),
			ParameterValue.FromWireValue([12]),
			Address(26, 100, 0)));
		var submitted = new RecordingRouterIngress();
		var receiver = new NodeManagerParticipantIngressReceiver(
			new RouterParameterRequestSettings(nodeManager, Address(26, 100, 0)),
			projectionSource,
			new RecordingUserAgentIngressReceiver(),
			submitted);

		await receiver.ReceiveAsync(
			CreateCurrentParameterRequest(Address(26, 100, 24), nodeManager, parameterNumber),
			CancellationToken.None);

		submitted.Envelope.Should().NotBeNull();
		var response = submitted.Envelope!;
		response.Source.Should().Be(nodeManager);
		response.Destinations.Addresses.Should().Equal(Address(26, 100, 24));
		response.Contents.Should().BeOfType<Parameter>().Which.ParameterValue
			.ToWireValue().Should().Equal([expectedValue]);
	}

	[Fact]
	public async Task Delegates_an_unrelated_envelope_to_the_response_correlator()
	{
		var nodeManager = Address(26, 100, 25);
		var responses = new RecordingUserAgentIngressReceiver();
		var projectionSource = new NodeManagerCurrentParameterProjectionSource();
		projectionSource.Publish(NodeManagerCurrentParameterProjection.FromNonVolatileParameters(
			ParameterValue.FromWireValue([25]),
			ParameterValue.FromWireValue([12]),
			Address(26, 100, 0)));
		var receiver = new NodeManagerParticipantIngressReceiver(
			new RouterParameterRequestSettings(nodeManager, Address(26, 100, 0)),
			projectionSource,
			responses,
			new RecordingRouterIngress());
		var envelope = Envelope.FromValues(
			Address(26, 100, 24),
			Destinations.FromAddresses(nodeManager),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(3)),
				AcknowledgementRequest.NotRequested),
			Acknowledgement.Create());

		await receiver.ReceiveAsync(envelope, CancellationToken.None);

		responses.Envelope.Should().BeSameAs(envelope);
	}

	private static Envelope CreateCurrentParameterRequest(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		byte parameterNumber)
	{
		return Envelope.FromValues(
			source,
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(parameterNumber)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(parameterNumber)));
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class RecordingRouterIngress : IRouterIngress
	{
		public Envelope? Envelope { get; private set; }

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class RecordingUserAgentIngressReceiver : IUserAgentIngressReceiver
	{
		public Envelope? Envelope { get; private set; }

		public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return Task.CompletedTask;
		}
	}
}
