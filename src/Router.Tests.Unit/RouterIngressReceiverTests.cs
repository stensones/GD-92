using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Router.Tests.Unit;

public sealed class RouterIngressReceiverTests
{
	[Fact]
	public async Task Acknowledges_a_level_one_password_logon_and_records_the_supplied_communications_address()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var requestSource = CreateAddress(26, 100, 25);
		var suppliedCommunicationsAddress = CreateAddress(42, 200, 7);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(CreateCurrentParameterProjection(routerAddress));
		var ingress = new CapturingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
				currentParameters),
			ingress,
			NullLogger<RouterIngressReceiver>.Instance);

		await receiver.ReceiveAsync(
			CreatePasswordLogon(
				requestSource,
				routerAddress,
				suppliedCommunicationsAddress,
				"FIRE"),
			CancellationToken.None);

		ingress.Envelope.Should().NotBeNull();
		var response = ingress.Envelope!;
		response.Source.Should().Be(routerAddress);
		response.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(requestSource);
		response.ProtocolAndPriority.Priority.Should().Be(
			MessagePriority.FromValue(MessagePriorityLevel.FromValue(2)));
		response.AcknowledgementAndSequence.SequenceNumber.Should().Be(
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(8)));
		response.AcknowledgementAndSequence.AcknowledgementRequest.Should().Be(
			AcknowledgementRequest.NotRequested);
		response.Contents.Should().BeOfType<Acknowledgement>();

		var currentPassword = currentParameters.GetCurrent().CurrentPassword;
		currentPassword.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.FromValue(1)));
		currentPassword.Password.Value.Value.Value.Should().BeEmpty();
		currentPassword.CommunicationsAddress.Should().Be(suppliedCommunicationsAddress);
	}

	[Fact]
	public async Task Delivers_the_Router_response_to_User_Agent_ingress_before_completing()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var ingress = new BlockingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			ingress,
			NullLogger<RouterIngressReceiver>.Instance);

		var receive = receiver.ReceiveAsync(CreateEnvelope(routerAddress), CancellationToken.None);

		await ingress.Delivered.Task;
		receive.IsCompleted.Should().BeFalse();
		ingress.Envelope.Should().NotBeNull();
		ingress.Envelope!.Contents.Should().BeOfType<Parameter>();

		ingress.CompleteDelivery();
		await receive;
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

	private static Envelope CreatePasswordLogon(
		CommunicationsAddress source,
		CommunicationsAddress routerAddress,
		CommunicationsAddress suppliedCommunicationsAddress,
		string password)
	{
		return Envelope.FromValues(
			source,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(2)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(8)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue(
					PasswordParameter.FromFields(
						PasswordLevel.FromValue(PasswordLevelNumber.FromValue(1)),
						Password.FromValue(
							PasswordValue.FromValue(SevenBitAsciiString.FromValue(password))),
						suppliedCommunicationsAddress).ToWireValue())));
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private static RouterCurrentParameterProjection CreateCurrentParameterProjection(
		CommunicationsAddress localAddress)
	{
		return RouterCurrentParameterProjection.FromNonVolatileValues(
			ParameterValue.FromWireValue(localAddress.Brigade.Value.ToWireValue()),
			ParameterValue.FromWireValue(
				PasswordParameter.FromFields(
					PasswordLevel.FromValue(PasswordLevelNumber.FromValue(0)),
					Password.FromValue(
						PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
					localAddress).ToWireValue()),
			PasswordVerifier.Create(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
				PasswordVerifierWorkFactor.Default),
			ParameterValue.FromWireValue([5]),
			ParameterValue.FromWireValue([3]));
	}

	private sealed class CapturingUserAgentIngress : IUserAgentIngress
	{
		public Envelope? Envelope { get; private set; }

		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class BlockingUserAgentIngress : IUserAgentIngress
	{
		private readonly TaskCompletionSource deliveryCompleted = new();

		public TaskCompletionSource Delivered { get; } = new();
		public Envelope? Envelope { get; private set; }

		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			this.Delivered.TrySetResult();
			return this.deliveryCompleted.Task.WaitAsync(cancellationToken);
		}

		public void CompleteDelivery()
		{
			this.deliveryCompleted.TrySetResult();
		}
	}
}
