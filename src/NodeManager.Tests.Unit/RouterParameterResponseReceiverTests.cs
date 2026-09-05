using AwesomeAssertions;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Tests.Unit;

public sealed class RouterParameterResponseReceiverTests
{
	[Fact]
	public async Task Completes_the_matching_pending_request_with_its_Parameter_Value()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.Reserve(userAgent, router);
		var receiver = new RouterParameterResponseReceiver(pendingDeliveries);

		await receiver.ReceiveAsync(CreateParameterResponse(router, userAgent, identifier.USWR.SequenceNumber), CancellationToken.None);

		pendingDeliveries.IsPending(identifier).Should().BeFalse();
		var status = pendingDeliveries.GetStatus(identifier)
			.Should().BeOfType<ReceivedRouterParameterRequestStatus>().Which;
		status.ParameterValue.ToWireValue().Should().Equal([26]);
	}

	[Fact]
	public async Task Completes_a_matching_logon_with_its_supplied_User_Agent_address()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.ReserveNodeLogin(userAgent, router, userAgent);
		var receiver = new RouterParameterResponseReceiver(pendingDeliveries);

		await receiver.ReceiveAsync(
			Envelope.FromValues(
				router,
				Destinations.FromAddresses(userAgent),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
				AcknowledgementAndSequence.FromValues(
					identifier.USWR.SequenceNumber,
					AcknowledgementRequest.NotRequested),
				Acknowledgement.Create()),
			CancellationToken.None);

		pendingDeliveries.IsPending(identifier).Should().BeFalse();
		var status = pendingDeliveries.GetStatus(identifier)
			.Should().BeOfType<LoggedOnNodeLoginStatus>().Which;
		status.UserAgentAddress.Should().Be(userAgent);
		status.State.Should().Be("logged-on");
	}

	[Theory]
	[InlineData(true, false, false)]
	[InlineData(false, true, false)]
	[InlineData(false, false, true)]
	public async Task Leaves_a_pending_request_unchanged_when_response_correlation_does_not_match(
		bool wrongSource,
		bool wrongDestination,
		bool wrongSequence)
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.Reserve(userAgent, router);
		var receiver = new RouterParameterResponseReceiver(pendingDeliveries);

		await receiver.ReceiveAsync(
			CreateParameterResponse(
				wrongSource ? Address(brigade: 26, node: 100, port: 1) : router,
				wrongDestination ? Address(brigade: 26, node: 100, port: 26) : userAgent,
				wrongSequence
					? SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1))
					: identifier.USWR.SequenceNumber),
			CancellationToken.None);

		pendingDeliveries.IsPending(identifier).Should().BeTrue();
		pendingDeliveries.GetStatus(identifier).Should().BeOfType<PendingRouterParameterRequestStatus>();
	}

	[Fact]
	public async Task Leaves_a_pending_request_unchanged_for_a_non_Parameter_response()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.Reserve(userAgent, router);
		var receiver = new RouterParameterResponseReceiver(pendingDeliveries);
		var acknowledgement = Envelope.CreateAcknowledgement(
			Envelope.FromValues(
				userAgent,
				Destinations.FromAddresses(router),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
				AcknowledgementAndSequence.FromValues(
					identifier.USWR.SequenceNumber,
					AcknowledgementRequest.Requested),
				ParameterRequest.FromFields(ParameterTable.Current, ParameterNumber.FromValue(1))),
			router,
			ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)));

		await receiver.ReceiveAsync(acknowledgement, CancellationToken.None);

		pendingDeliveries.IsPending(identifier).Should().BeTrue();
	}

	private static Envelope CreateParameterResponse(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		SequenceNumber sequenceNumber)
	{
		return Envelope.FromValues(
			source,
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
