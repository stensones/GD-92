using AwesomeAssertions;
using Router.Persistence;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Envelope = Stensones.GD92.Messages.Envelope;

namespace Router.Tests.Unit;

public sealed class RouterParameterRequestHandlerTests
{
	[Fact]
	public void Returns_the_local_brigade_identifier_for_current_parameter_one()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var routerProtocolVersion = ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(1));
		var requestSource = CreateAddress(26, 100, 25);
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			routerProtocolVersion);

		var result = handler.Handle(CreateParameterRequest(requestSource, routerAddress));

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.Responded);
		result.Response.Should().NotBeNull();
		var response = result.Response!;
		response.Source.Should().Be(routerAddress);
		response.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(requestSource);
		response.ProtocolAndPriority.Priority.Should().Be(
			MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)));
		response.ProtocolAndPriority.ProtocolVersion.Should().Be(routerProtocolVersion);
		response.AcknowledgementAndSequence.SequenceNumber.Should().Be(
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)));
		response.AcknowledgementAndSequence.AcknowledgementRequest.Should().Be(AcknowledgementRequest.NotRequested);
		var parameter = response.Contents.Should().BeOfType<Parameter>().Subject;
		parameter.MoreValues.Should().Be(MoreValues.No);
		parameter.ParameterValue.ToWireValue().Should().Equal(new byte[] { 26 });
	}

	[Fact]
	public void Returns_current_parameter_one_from_its_current_parameter_projection()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var requestSource = CreateAddress(26, 100, 25);
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
			CreateCurrentParameterProjection(42));

		var result = handler.Handle(CreateParameterRequest(requestSource, routerAddress));

		result.Response.Should().NotBeNull();
		var response = result.Response!;
		var parameter = response.Contents.Should().BeOfType<Parameter>().Subject;
		parameter.ParameterValue.ToWireValue().Should().Equal(new byte[] { 42 });
	}

	[Fact]
	public void Returns_current_parameter_one_from_the_projection_published_at_startup()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var currentParameters = new RouterCurrentParameterProjectionSource();
		currentParameters.Publish(CreateCurrentParameterProjection(42));
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
			currentParameters);

		var result = handler.Handle(CreateParameterRequest(CreateAddress(26, 100, 25), routerAddress));

		result.Response.Should().NotBeNull();
		var parameter = result.Response!.Contents.Should().BeOfType<Parameter>().Subject;
		parameter.ParameterValue.ToWireValue().Should().Equal([42]);
	}

	[Fact]
	public void Rejects_an_invalid_current_parameter_four_level_one_password_with_the_parameter_invalid_password_NAK()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(CreateCurrentParameterProjection(
			26,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1"))));
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
			source);
		var userAgentAddress = CreateAddress(26, 100, 25);
		var request = Envelope.FromValues(
			userAgentAddress,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue(
					PasswordParameter.FromFields(
						PasswordLevel.FromValue(PasswordLevelNumber.Level1),
						Password.FromValue(
							PasswordValue.FromValue(SevenBitAsciiString.FromValue("WATER"))),
						userAgentAddress).ToWireValue())));

		var result = handler.Handle(request);

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.Responded);
		result.Response.Should().NotBeNull();
		var response = result.Response!;
		response.Source.Should().Be(routerAddress);
		response.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(userAgentAddress);
		response.AcknowledgementAndSequence.SequenceNumber.Should().Be(request.AcknowledgementAndSequence.SequenceNumber);
		var negativeAcknowledgement = response.Contents.Should().BeOfType<NegativeAcknowledgement>().Subject;
		negativeAcknowledgement.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(routerAddress);
		negativeAcknowledgement.ReasonCode.ToWireValue().Should().Equal([0x04, 0x04]);
	}

	[Fact]
	public void Acknowledges_a_level_zero_current_password_and_clears_the_active_Node_Login()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var source = new RouterCurrentParameterProjectionSource();
		source.Publish(CreateCurrentParameterProjection(
			26,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1"))));
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)),
			source);
		var requestSource = CreateAddress(26, 100, 25);
		source.TryLogOnAtLevelOne(PasswordParameter.FromFields(
			PasswordLevel.FromValue(PasswordLevelNumber.Level1),
			Password.FromValue(
				PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1"))),
			requestSource)).Should().BeTrue();
		var ignoredAddress = CreateAddress(42, 200, 7);
		var request = Envelope.FromValues(
			requestSource,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue(
					PasswordParameter.FromFields(
						PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
						Password.FromValue(
							PasswordValue.FromValue(SevenBitAsciiString.FromValue("ignored"))),
						ignoredAddress).ToWireValue())));

		var result = handler.Handle(request);

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.Responded);
		result.Response!.Contents.Should().BeOfType<Acknowledgement>();
		var currentPassword = source.GetCurrent().CurrentPassword;
		currentPassword.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated));
		currentPassword.Password.Value.Value.Value.Should().BeEmpty();
		currentPassword.CommunicationsAddress.Should().Be(routerAddress);
	}

	[Fact]
	public void Does_not_respond_to_a_Parameter_Request_addressed_outside_the_local_Router()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)));

		var result = handler.Handle(CreateParameterRequest(
			CreateAddress(26, 100, 25),
			CreateAddress(26, 100, 1)));

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.DestinationNotHandled);
		result.Response.Should().BeNull();
	}

	[Fact]
	public void Does_not_respond_to_a_Parameter_Request_with_multiple_destinations()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)));

		var result = handler.Handle(CreateParameterRequest(
			CreateAddress(26, 100, 25),
			routerAddress,
			CreateAddress(26, 100, 1)));

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.DestinationNotHandled);
		result.Response.Should().BeNull();
	}

	[Fact]
	public void Does_not_respond_to_a_non_Parameter_Request()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)));
		var source = CreateAddress(26, 100, 25);

		var result = handler.Handle(Envelope.FromValues(
			source,
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26]))));

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.MessageTypeNotHandled);
		result.Response.Should().BeNull();
	}

	[Fact]
	public void Does_not_respond_to_an_unsupported_Parameter_Request()
	{
		var routerAddress = CreateAddress(26, 100, 0);
		var handler = new RouterParameterRequestHandler(
			routerAddress,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)));

		var result = handler.Handle(Envelope.FromValues(
			CreateAddress(26, 100, 25),
			Destinations.FromAddresses(routerAddress),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7)),
				AcknowledgementRequest.Requested),
			ParameterRequest.FromFields(ParameterTable.Permanent, ParameterNumber.FromValue(1))));

		result.Status.Should().Be(RouterEnvelopeHandlingStatus.ParameterNotHandled);
		result.Response.Should().BeNull();
	}

	private static Envelope CreateParameterRequest(
		CommunicationsAddress source,
		params CommunicationsAddress[] destinations)
	{
		return Envelope.FromValues(
			source,
			Destinations.FromAddresses(destinations),
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

	private static RouterCurrentParameterProjection CreateCurrentParameterProjection(
		byte brigade,
		PasswordValue? levelOnePassword = null)
	{
		var localAddress = CreateAddress(brigade, 100, 0);

		return RouterCurrentParameterProjection.FromNonVolatileValues(
			ParameterValue.FromWireValue([brigade]),
			ParameterValue.FromWireValue(
				PasswordParameter.FromFields(
					PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
					Password.FromValue(
						PasswordValue.FromValue(SevenBitAsciiString.FromValue(string.Empty))),
					localAddress).ToWireValue()),
			PasswordVerifier.Create(
				levelOnePassword ?? PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE")),
				PasswordVerifierWorkFactor.Default),
			ParameterValue.FromWireValue([5]),
			ParameterValue.FromWireValue([3]));
	}
}
