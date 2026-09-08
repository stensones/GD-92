using AwesomeAssertions;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Router.Tests.Unit;

public sealed class RouterParameterRequestHandlerTests
{
	[Fact]
	public async Task Dispatches_a_local_Current_Parameter_Request_to_the_Router_Parameter_Read()
	{
		var localAddress = CreateAddress(26, 100, 0);
		var parameterRead = new CapturingRouterParameterRead();
		var handler = CreateHandler(localAddress, parameterRead, new CapturingNodeLogin(), new CapturingLevel1PasswordModification());
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(localAddress),
			CreateProtocolAndPriority(),
			CreateAcknowledgementAndSequence(),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1)));

		var result = await handler.HandleAsync(request, CancellationToken.None);

		result.Should().BeSameAs(parameterRead.Result);
		parameterRead.Envelope.Should().BeSameAs(request);
	}

	[Fact]
	public async Task Dispatches_a_local_Current_Password_to_Node_Login()
	{
		var localAddress = CreateAddress(26, 100, 0);
		var nodeLogin = new CapturingNodeLogin();
		var handler = CreateHandler(localAddress, new CapturingRouterParameterRead(), nodeLogin, new CapturingLevel1PasswordModification());
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(localAddress),
			CreateProtocolAndPriority(),
			CreateAcknowledgementAndSequence(),
			SetParameter.FromFields(
				ParameterTable.Current,
				RouterParameterCatalogue.CurrentPassword.Number,
				ParameterValue.FromWireValue([0])));

		var result = await handler.HandleAsync(request, CancellationToken.None);

		result.Should().BeSameAs(nodeLogin.Result);
		nodeLogin.Envelope.Should().BeSameAs(request);
	}

	[Fact]
	public async Task Dispatches_a_local_Level1_Password_change_to_Level1_Password_Modification()
	{
		var localAddress = CreateAddress(26, 100, 0);
		var passwordModification = new CapturingLevel1PasswordModification();
		var handler = CreateHandler(localAddress, new CapturingRouterParameterRead(), new CapturingNodeLogin(), passwordModification);
		var request = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(localAddress),
			CreateProtocolAndPriority(),
			CreateAcknowledgementAndSequence(),
			SetParameter.FromFields(
				ParameterTable.NonVolatile,
				RouterParameterCatalogue.Level1PasswordNumber,
				ParameterValue.FromWireValue([0])));

		var result = await handler.HandleAsync(request, CancellationToken.None);

		result.Should().BeSameAs(passwordModification.Result);
		passwordModification.Envelope.Should().BeSameAs(request);
	}

	[Fact]
	public async Task Does_not_dispatch_an_unaddressed_or_non_management_Envelope()
	{
		var localAddress = CreateAddress(26, 100, 0);
		var parameterRead = new CapturingRouterParameterRead();
		var nodeLogin = new CapturingNodeLogin();
		var passwordModification = new CapturingLevel1PasswordModification();
		var handler = CreateHandler(localAddress, parameterRead, nodeLogin, passwordModification);
		var unaddressedRequest = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(CreateAddress(26, 100, 1)),
			CreateProtocolAndPriority(),
			CreateAcknowledgementAndSequence(),
			ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1)));
		var nonManagementEnvelope = Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(localAddress),
			CreateProtocolAndPriority(),
			CreateAcknowledgementAndSequence(),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));

		var destinationResult = await handler.HandleAsync(unaddressedRequest, CancellationToken.None);
		var messageTypeResult = await handler.HandleAsync(nonManagementEnvelope, CancellationToken.None);

		destinationResult.Status.Should().Be(RouterEnvelopeHandlingStatus.DestinationNotHandled);
		messageTypeResult.Status.Should().Be(RouterEnvelopeHandlingStatus.MessageTypeNotHandled);
		parameterRead.Envelope.Should().BeNull();
		nodeLogin.Envelope.Should().BeNull();
		passwordModification.Envelope.Should().BeNull();
	}

	private static RouterParameterRequestHandler CreateHandler(
		CommunicationsAddress localAddress,
		IRouterParameterRead parameterRead,
		INodeLogin nodeLogin,
		ILevel1PasswordModification passwordModification)
	{
		return new RouterParameterRequestHandler(
			localAddress,
			parameterRead,
			nodeLogin,
			passwordModification);
	}

	private static CommunicationsAddress CreateAddress(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private static ProtocolAndPriority CreateProtocolAndPriority()
	{
		return ProtocolAndPriority.FromValues(
			MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)));
	}

	private static AcknowledgementAndSequence CreateAcknowledgementAndSequence()
	{
		return AcknowledgementAndSequence.FromValues(
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
			AcknowledgementRequest.Requested);
	}

	private sealed class CapturingRouterParameterRead : IRouterParameterRead
	{
		public Envelope? Envelope { get; private set; }
		public RouterEnvelopeHandlingResult Result { get; } =
			RouterEnvelopeHandlingResult.NotHandled(RouterEnvelopeHandlingStatus.ParameterNotHandled);

		public ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
			Envelope envelope,
			CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return ValueTask.FromResult(this.Result);
		}
	}

	private sealed class CapturingNodeLogin : INodeLogin
	{
		public Envelope? Envelope { get; private set; }
		public RouterEnvelopeHandlingResult Result { get; } =
			RouterEnvelopeHandlingResult.NotHandled(RouterEnvelopeHandlingStatus.ParameterNotHandled);

		public ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
			Envelope envelope,
			CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return ValueTask.FromResult(this.Result);
		}
	}

	private sealed class CapturingLevel1PasswordModification : ILevel1PasswordModification
	{
		public Envelope? Envelope { get; private set; }
		public RouterEnvelopeHandlingResult Result { get; } =
			RouterEnvelopeHandlingResult.NotHandled(RouterEnvelopeHandlingStatus.ParameterNotHandled);

		public ValueTask<RouterEnvelopeHandlingResult> HandleAsync(
			Envelope envelope,
			CancellationToken cancellationToken)
		{
			this.Envelope = envelope;
			return ValueTask.FromResult(this.Result);
		}
	}
}
