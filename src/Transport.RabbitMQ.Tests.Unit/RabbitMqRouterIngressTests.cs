using AwesomeAssertions;
using System.Reflection;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Wolverine;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class RabbitMqRouterIngressTests
{
	[Fact]
	public async Task Submits_exact_Envelope_wire_bytes_to_the_configured_local_Router()
	{
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var envelope = Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 101, port: 0)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1)));
		var destinationEndpoint = DispatchProxy.Create<IDestinationEndpoint, RecordingDestinationEndpoint>();
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();
		((RecordingDestinationEndpoint)(object)destinationEndpoint).Endpoint = destinationEndpoint;
		((RecordingMessageBus)(object)messageBus).DestinationEndpoint = destinationEndpoint;
		var ingress = new RabbitMqRouterIngress(messageBus, localRouter);

		await ingress.SubmitAsync(envelope, CancellationToken.None);

		var recordingBus = (RecordingMessageBus)(object)messageBus;
		recordingBus.EndpointUri.Should().Be(RouterIngressEndpoint.From(localRouter));
		var transportMessage = ((RecordingDestinationEndpoint)(object)destinationEndpoint).SentMessage
			.Should().BeOfType<RouterIngressTransportMessage>().Which;
		transportMessage.EnvelopeWireValue.Should().Equal(envelope.ToWireValue());
		transportMessage.Write().Should().Equal(envelope.ToWireValue());
	}

	[Fact]
	public async Task Does_not_submit_when_the_request_is_cancelled()
	{
		var destinationEndpoint = DispatchProxy.Create<IDestinationEndpoint, RecordingDestinationEndpoint>();
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();
		((RecordingDestinationEndpoint)(object)destinationEndpoint).Endpoint = destinationEndpoint;
		((RecordingMessageBus)(object)messageBus).DestinationEndpoint = destinationEndpoint;
		var ingress = new RabbitMqRouterIngress(messageBus, Address(brigade: 26, node: 100, port: 0));
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		var submit = () => ingress.SubmitAsync(CreateEnvelope(), cancellation.Token);

		await submit.Should().ThrowAsync<OperationCanceledException>();
		((RecordingMessageBus)(object)messageBus).EndpointUri.Should().BeNull();
	}

	[Fact]
	public void Rejects_missing_Router_configuration()
	{
		var messageBus = DispatchProxy.Create<IMessageBus, RecordingMessageBus>();

		var createIngress = () => new RabbitMqRouterIngress(messageBus, null!);

		createIngress.Should().Throw<ArgumentNullException>()
			.Which.ParamName.Should().Be("localRouter");
	}

	private static Envelope CreateEnvelope()
	{
		return Envelope.FromValues(
			Address(brigade: 26, node: 100, port: 25),
			Destinations.FromAddresses(Address(brigade: 26, node: 101, port: 0)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1)));
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
		public IDestinationEndpoint? Endpoint { get; set; }
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
