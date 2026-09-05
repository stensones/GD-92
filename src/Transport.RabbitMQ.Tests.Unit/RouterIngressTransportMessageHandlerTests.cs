using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Stensones.GD92.Transport.RabbitMQ.Tests.Unit;

public sealed class RouterIngressTransportMessageHandlerTests
{
	[Fact]
	public async Task Delivers_the_exact_decoded_Envelope_to_the_Router_receiver()
	{
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
		var receiver = new RecordingRouterIngressReceiver();
		var scopeFactory = new RecordingServiceScopeFactory(receiver);
		var handler = new RouterIngressTransportMessageHandler();

		await handler.HandleAsync(
			new RouterIngressTransportMessage(envelope.ToWireValue()),
			scopeFactory,
			CancellationToken.None);

		scopeFactory.CreatedScopeCount.Should().Be(1);
		scopeFactory.DisposedScopeCount.Should().Be(1);
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

	private sealed class RecordingRouterIngressReceiver : IRouterIngressReceiver
	{
		public Envelope? ReceivedEnvelope { get; private set; }

		public Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.ReceivedEnvelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class RecordingServiceScopeFactory(
		IRouterIngressReceiver receiver) : IServiceScopeFactory
	{
		public int CreatedScopeCount { get; private set; }
		public int DisposedScopeCount { get; private set; }

		public IServiceScope CreateScope()
		{
			this.CreatedScopeCount++;
			return new RecordingServiceScope(receiver, () => this.DisposedScopeCount++);
		}
	}

	private sealed class RecordingServiceScope(
		IRouterIngressReceiver receiver,
		Action dispose) : IServiceScope
	{
		public IServiceProvider ServiceProvider { get; } =
			new RouterIngressReceiverServiceProvider(receiver);

		public void Dispose()
		{
			dispose();
		}
	}

	private sealed class RouterIngressReceiverServiceProvider(
		IRouterIngressReceiver receiver) : IServiceProvider
	{
		public object? GetService(Type serviceType)
		{
			return serviceType == typeof(IRouterIngressReceiver) ? receiver : null;
		}
	}
}
