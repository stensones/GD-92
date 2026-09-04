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
			RouterCurrentParameterProjection.FromBrigadeOrAgencyIdentifier(
				BrigadeOrAgencyIdentifier.FromValue(42)));

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
		currentParameters.Publish(Router.Persistence.RouterCurrentParameterProjection.FromParameterOneValue(
			ParameterValue.FromWireValue([42])));
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
}
