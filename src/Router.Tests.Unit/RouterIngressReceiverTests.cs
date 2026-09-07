using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ParticipantParameters;
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
			new CapturingLocalParticipantIngress(),
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
			PasswordLevel.FromValue(PasswordLevelNumber.Level1));
		currentPassword.Password.Value.Value.Value.Should().BeEmpty();
		currentPassword.CommunicationsAddress.Should().Be(suppliedCommunicationsAddress);
	}

	[Fact]
	public async Task An_active_level_one_Node_Login_can_change_the_current_Level1_password()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var requestSource = CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(CreateCurrentParameterProjection(routerAddress));
		currentParameters.TryLogOnAtLevelOne(CreatePasswordParameter(
			PasswordLevelNumber.Level1,
			"FIRE",
			requestSource)).Should().BeTrue();
		var passwordVerifierStore = new CapturingPasswordVerifierStore();
		var ingress = new CapturingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
				currentParameters,
				passwordVerifierStore),
			ingress,
			new CapturingLocalParticipantIngress(),
			NullLogger<RouterIngressReceiver>.Instance);

		await receiver.ReceiveAsync(
			CreateLevel1PasswordChange(
				requestSource,
				routerAddress,
				ParameterTable.Current,
				"WATER"),
			CancellationToken.None);

		ingress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"))).Should().BeTrue();
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"))).Should().BeFalse();
		currentParameters.GetCurrent().CurrentPassword.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Level1));
		currentParameters.GetCurrent().CurrentPassword.CommunicationsAddress.Should().Be(requestSource);
		currentParameters.GetCurrent().CurrentPassword.Password.Value.Value.Value.Should().BeEmpty();
		passwordVerifierStore.StoredValues.Should().BeEmpty();
	}

	[Fact]
	public async Task An_active_level_one_Node_Login_stores_a_non_volatile_Level1_password_for_the_next_restart()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var passwordVerifierStore = new CapturingPasswordVerifierStore();
		var bootstrapper = new RouterParameterBootstrapper(
			new InMemoryRouterParameterStore(),
			passwordVerifierStore);
		var initialPassword = PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"));
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(await bootstrapper.LoadCurrentParameterProjectionAsync(
			RouterParameterBootstrapConfiguration.FromValues(
				routerAddress,
				initialPassword,
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
				Retries.FromValue(Word8.FromValue(3)))));
		var requestSource = CreateAddress(26, 100, 25);
		currentParameters.TryLogOnAtLevelOne(CreatePasswordParameter(
			PasswordLevelNumber.Level1,
			"FIRE",
			requestSource)).Should().BeTrue();
		var ingress = new CapturingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
				currentParameters,
				passwordVerifierStore),
			ingress,
			new CapturingLocalParticipantIngress(),
			NullLogger<RouterIngressReceiver>.Instance);

		await receiver.ReceiveAsync(
			CreateLevel1PasswordChange(
				requestSource,
				routerAddress,
				ParameterTable.NonVolatile,
				"WATER"),
			CancellationToken.None);

		ingress.Envelope!.Contents.Should().BeOfType<Acknowledgement>();
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(initialPassword).Should().BeTrue();
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"))).Should().BeFalse();
		(await bootstrapper.LoadCurrentParameterProjectionAsync(
			RouterParameterBootstrapConfiguration.FromValues(
				routerAddress,
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("INITIAL")),
				NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
				Retries.FromValue(Word8.FromValue(3)))))
			.Level1PasswordVerifier.Verifies(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"))).Should().BeTrue();
	}

	[Fact]
	public async Task A_permanent_Level1_password_change_is_rejected_with_no_modification_access()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var requestSource = CreateAddress(26, 100, 25);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(CreateCurrentParameterProjection(routerAddress));
		currentParameters.TryLogOnAtLevelOne(CreatePasswordParameter(
			PasswordLevelNumber.Level1,
			"FIRE",
			requestSource)).Should().BeTrue();
		var passwordVerifierStore = new CapturingPasswordVerifierStore();
		var ingress = new CapturingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
				currentParameters,
				passwordVerifierStore),
			ingress,
			new CapturingLocalParticipantIngress(),
			NullLogger<RouterIngressReceiver>.Instance);

		await receiver.ReceiveAsync(
			CreateLevel1PasswordChange(
				requestSource,
				routerAddress,
				ParameterTable.Permanent,
				"WATER"),
			CancellationToken.None);

		var negativeAcknowledgement = ingress.Envelope!.Contents
			.Should().BeOfType<NegativeAcknowledgement>().Subject;
		negativeAcknowledgement.ReasonCode.ParameterReasonCode.Should().Be(
			ParameterReasonCode.NoModificationAccess);
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"))).Should().BeTrue();
		passwordVerifierStore.StoredValues.Should().BeEmpty();
	}

	[Fact]
	public async Task A_Level1_password_change_requires_an_active_Level1_Node_Login()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(CreateCurrentParameterProjection(routerAddress));
		var passwordVerifierStore = new CapturingPasswordVerifierStore();
		var ingress = new CapturingUserAgentIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
				currentParameters,
				passwordVerifierStore),
			ingress,
			new CapturingLocalParticipantIngress(),
			NullLogger<RouterIngressReceiver>.Instance);

		await receiver.ReceiveAsync(
			CreateLevel1PasswordChange(
				CreateAddress(26, 100, 25),
				routerAddress,
				ParameterTable.Current,
				"WATER"),
			CancellationToken.None);

		var negativeAcknowledgement = ingress.Envelope!.Contents
			.Should().BeOfType<NegativeAcknowledgement>().Subject;
		negativeAcknowledgement.ReasonCode.ParameterReasonCode.Should().Be(
			ParameterReasonCode.NoModificationAccess);
		currentParameters.GetCurrent().Level1PasswordVerifier.Verifies(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE"))).Should().BeTrue();
		passwordVerifierStore.StoredValues.Should().BeEmpty();
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
			new CapturingLocalParticipantIngress(),
			NullLogger<RouterIngressReceiver>.Instance);

		var receive = receiver.ReceiveAsync(CreateEnvelope(routerAddress), CancellationToken.None);

		await ingress.Delivered.Task;
		receive.IsCompleted.Should().BeFalse();
		ingress.Envelope.Should().NotBeNull();
		ingress.Envelope!.Contents.Should().BeOfType<Parameter>();

		ingress.CompleteDelivery();
		await receive;
	}

	[Fact]
	public async Task Delivers_a_single_non_Router_local_destination_to_Local_Participant_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var request = CreateEnvelope(CreateAddress(26, 100, 1));

		await receiver.ReceiveAsync(request, CancellationToken.None);

		localParticipantIngress.Envelope.Should().Be(request);
		userAgentIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_local_acknowledgement_to_User_Agent_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var acknowledgement = Envelope.FromValues(
			CreateAddress(26, 100, 1),
			Destinations.FromAddresses(CreateAddress(26, 100, 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Acknowledgement.Create());

		await receiver.ReceiveAsync(acknowledgement, CancellationToken.None);

		userAgentIngress.Envelope.Should().Be(acknowledgement);
		localParticipantIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_local_Parameter_Message_to_User_Agent_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var parameter = Envelope.FromValues(
			CreateAddress(26, 100, 1),
			Destinations.FromAddresses(CreateAddress(26, 100, 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([12])));

		await receiver.ReceiveAsync(parameter, CancellationToken.None);

		userAgentIngress.Envelope.Should().Be(parameter);
		localParticipantIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_local_Negative_Acknowledgement_to_User_Agent_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var negativeAcknowledgement = Envelope.FromValues(
			CreateAddress(26, 100, 1),
			Destinations.FromAddresses(CreateAddress(26, 100, 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			NegativeAcknowledgement.FromValues(
				Destinations.FromAddresses(CreateAddress(26, 100, 25)),
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax)));

		await receiver.ReceiveAsync(negativeAcknowledgement, CancellationToken.None);

		userAgentIngress.Envelope.Should().Be(negativeAcknowledgement);
		localParticipantIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_local_Text_Message_to_User_Agent_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var message = Envelope.FromValues(
			CreateAddress(26, 100, 1),
			Destinations.FromAddresses(CreateAddress(26, 100, 25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Stensones.GD92.Messages.Text.FromFields(
				Block.FromValue(1),
				OfBlocks.FromValue(1),
				Stensones.GD92.Fields.Text.FromValue("FIRE")));

		await receiver.ReceiveAsync(message, CancellationToken.None);

		userAgentIngress.Envelope.Should().Be(message);
		localParticipantIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_single_local_Parameter_Request_Multiple_to_Local_Participant_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(CreateAddress(26, 100, 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequestMultiple.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterEntrySelection.MostRecent(ParameterEntryCount.FromValue(1))));

		await receiver.ReceiveAsync(request, CancellationToken.None);

		localParticipantIngress.Envelope.Should().Be(request);
		userAgentIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Delivers_a_single_local_Set_Parameter_to_Local_Participant_Ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(CreateAddress(26, 100, 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterValue.FromWireValue([1])));

		await receiver.ReceiveAsync(request, CancellationToken.None);

		localParticipantIngress.Envelope.Should().Be(request);
		userAgentIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Does_not_deliver_a_multi_destination_Parameter_Request_to_either_local_ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(
				CreateAddress(26, 100, 1),
				CreateAddress(26, 100, 2)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(2)));

		await receiver.ReceiveAsync(request, CancellationToken.None);

		userAgentIngress.Envelope.Should().BeNull();
		localParticipantIngress.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Does_not_deliver_a_non_local_Set_Parameter_to_either_local_ingress()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var userAgentIngress = new CapturingUserAgentIngress();
		var localParticipantIngress = new CapturingLocalParticipantIngress();
		var receiver = new RouterIngressReceiver(
			new RouterParameterRequestHandler(
				routerAddress,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			userAgentIngress,
			localParticipantIngress,
			NullLogger<RouterIngressReceiver>.Instance);
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(CreateAddress(26, 101, 1)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(100),
				ParameterValue.FromWireValue([1])));

		await receiver.ReceiveAsync(request, CancellationToken.None);

		userAgentIngress.Envelope.Should().BeNull();
		localParticipantIngress.Envelope.Should().BeNull();
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
						PasswordLevel.FromValue(PasswordLevelNumber.Level1),
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
		return RouterCurrentParameterProjection.FromNonVolatileParameters(
			localAddress.Brigade.Value,
			PasswordParameter.FromFields(
				PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
				Password.FromValue(
					PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
				localAddress),
			PasswordVerifier.Create(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
				PasswordVerifierWorkFactor.Default),
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			Retries.FromValue(Word8.FromValue(3)));
	}

	private static PasswordParameter CreatePasswordParameter(
		PasswordLevelNumber level,
		string password,
		CommunicationsAddress communicationsAddress)
	{
		return PasswordParameter.FromFields(
			PasswordLevel.FromValue(level),
			Password.FromValue(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue(password))),
			communicationsAddress);
	}

	private static Envelope CreateLevel1PasswordChange(
		CommunicationsAddress source,
		CommunicationsAddress routerAddress,
		ParameterTable parameterTable,
		string newPassword)
	{
		return Envelope.FromValues(
			source,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(2)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(9)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				parameterTable,
				ParameterNumber.FromValue(5),
				ParameterValue.FromWireValue(
					Password.FromValue(
						PasswordValue.FromValue(SevenBitAsciiString.FromValue(newPassword)))
						.ToWireValue())));
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

	private sealed class CapturingLocalParticipantIngress : ILocalParticipantIngress
	{
		public Envelope? Envelope { get; private set; }

		public Task DeliverAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class CapturingPasswordVerifierStore : IRouterLevel1PasswordVerifierStore
	{
		public Dictionary<ParameterTable, PasswordVerifier> StoredValues { get; } = [];

		public ValueTask<PasswordVerifier?> GetAsync(
			ParameterTable parameterTable,
			CancellationToken cancellationToken = default)
		{
			this.StoredValues.TryGetValue(parameterTable, out var value);
			return ValueTask.FromResult(value);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			PasswordVerifier passwordVerifier,
			CancellationToken cancellationToken = default)
		{
			this.StoredValues[parameterTable] = passwordVerifier;
			return ValueTask.CompletedTask;
		}
	}

	private sealed class InMemoryRouterParameterStore :
		IParticipantParameterStore
	{
		private readonly Dictionary<(ParameterTable Table, ParameterNumber Number), ParameterValue> values = [];

		public ValueTask<ParameterValue?> GetAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken = default)
		{
			this.values.TryGetValue((parameterTable, parameterNumber), out var value);
			return ValueTask.FromResult(value);
		}

		public ValueTask StoreAsync(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterValue parameterValue,
			CancellationToken cancellationToken = default)
		{
			this.values[(parameterTable, parameterNumber)] = parameterValue;
			return ValueTask.CompletedTask;
		}

		public ValueTask<T> ExecuteInitializationAsync<T>(
			Func<CancellationToken, ValueTask<T>> initialize,
			CancellationToken cancellationToken = default)
		{
			return initialize(cancellationToken);
		}
	}
}
