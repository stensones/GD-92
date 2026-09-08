using System.Reflection;
using AwesomeAssertions;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class RabbitMqLocalParticipantIngressTests
{
	[Fact]
	public async Task Delivers_exact_Envelope_wire_bytes_to_its_single_local_Participant_destination()
	{
		var participant = Address(brigade: 26, node: 100, port: 12);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
			Destinations.FromAddresses(participant),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(2)));
		var destinationEndpoint = DispatchProxy.Create<IDestinationEndpoint, RecordingDestinationEndpoint>();
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();
		((RecordingMessageBus)(object)messageBus).DestinationEndpoint = destinationEndpoint;
		var ingress = new RabbitMqLocalParticipantIngress(messageBus);

		await ingress.DeliverAsync(envelope, CancellationToken.None);

		var recordingBus = (RecordingMessageBus)(object)messageBus;
		recordingBus.EndpointUri.Should().Be(LocalParticipantIngressEndpoint.From(participant));
		var transportMessage = ((RecordingDestinationEndpoint)(object)destinationEndpoint).SentMessage
			.Should().BeOfType<LocalParticipantIngressTransportMessage>().Which;
		transportMessage.EnvelopeWireValue.Should().Equal(envelope.ToWireValue());
	}

	[Fact]
	public async Task Rejects_an_Envelope_with_multiple_local_Participant_destinations()
	{
		var messageBus = DispatchProxy.Create<IMessageBus, EndpointSelectingMessageBus>();
		var ingress = new RabbitMqLocalParticipantIngress(messageBus);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
			Destinations.FromAddresses(
				Address(brigade: 26, node: 100, port: 1),
				Address(brigade: 26, node: 100, port: 2)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(2)));

		var deliver = () => ingress.DeliverAsync(envelope, CancellationToken.None);

		await deliver.Should().ThrowAsync<ArgumentException>();
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private class EndpointSelectingMessageBus : DispatchProxy
	{
		protected override object? Invoke(MethodInfo? targetMethod, object?[]? arguments)
		{
			throw new Xunit.Sdk.XunitException("Local Participant Ingress selected an endpoint.");
		}
	}

	private class RecordingMessageBus : DispatchProxy
	{
		public IDestinationEndpoint? DestinationEndpoint { get; set; }
		public Uri? EndpointUri { get; private set; }

		protected override object? Invoke(MethodInfo? targetMethod, object?[]? arguments)
		{
			if (targetMethod?.Name == nameof(IMessageBus.EndpointFor) &&
				arguments?[0] is Uri endpointUri)
			{
				this.EndpointUri = endpointUri;
				return this.DestinationEndpoint;
			}

			throw new NotSupportedException(targetMethod?.Name);
		}
	}

	private class RecordingDestinationEndpoint : DispatchProxy
	{
		public object? SentMessage { get; private set; }

		protected override object? Invoke(MethodInfo? targetMethod, object?[]? arguments)
		{
			if (targetMethod?.Name == nameof(IDestinationEndpoint.SendAsync))
			{
				this.SentMessage = arguments?[0];
				return ValueTask.CompletedTask;
			}

			throw new NotSupportedException(targetMethod?.Name);
		}
	}
}
