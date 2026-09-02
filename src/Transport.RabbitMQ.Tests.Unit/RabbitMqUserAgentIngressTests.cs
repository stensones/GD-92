using System.Reflection;
using AwesomeAssertions;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class RabbitMqUserAgentIngressTests
{
	[Fact]
	public async Task Delivers_exact_Envelope_wire_bytes_to_its_single_User_Agent_destination()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
			Destinations.FromAddresses(userAgent),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));
		var destinationEndpoint = DispatchProxy.Create<IDestinationEndpoint, RecordingDestinationEndpoint>();
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();
		((RecordingMessageBus)(object)messageBus).DestinationEndpoint = destinationEndpoint;
		var ingress = new RabbitMqUserAgentIngress(messageBus);

		await ingress.DeliverAsync(envelope, CancellationToken.None);

		var recordingBus = (RecordingMessageBus)(object)messageBus;
		recordingBus.EndpointUri.Should().Be(UserAgentIngressEndpoint.From(userAgent));
		var transportMessage = ((RecordingDestinationEndpoint)(object)destinationEndpoint).SentMessage
			.Should().BeOfType<UserAgentIngressTransportMessage>().Which;
		transportMessage.EnvelopeWireValue.Should().Equal(envelope.ToWireValue());
	}

	[Fact]
	public async Task Rejects_an_Envelope_with_multiple_User_Agent_destinations()
	{
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();
		var ingress = new RabbitMqUserAgentIngress(messageBus);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 0),
			Destinations.FromAddresses(
				Address(brigade: 26, node: 100, port: 25),
				Address(brigade: 26, node: 100, port: 26)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));

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
