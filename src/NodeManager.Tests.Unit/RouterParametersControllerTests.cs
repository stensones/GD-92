using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit;

public sealed class RouterParametersControllerTests
{
	[Fact]
	public async Task Submits_a_local_router_brigade_request_and_returns_its_pending_USWR()
	{
		var messageOriginator = Address(brigade: 26, node: 100, port: 25);
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var ingress = new RecordingRouterIngress();
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		IRouterParameterRequestService service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(messageOriginator, localRouter),
			pendingDeliveries,
			ingress);

		var statusIdentifier = await service.RequestLocalRouterBrigadeOrAgencyNumber(CancellationToken.None);

		ingress.SubmittedEnvelope.Should().NotBeNull();
		var submittedEnvelope = ingress.SubmittedEnvelope!;
		submittedEnvelope.Source.Should().Be(messageOriginator);
		submittedEnvelope.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(localRouter);
		submittedEnvelope.ProtocolAndPriority.Priority.Value.Should().Be(3);
		submittedEnvelope.ProtocolAndPriority.ProtocolVersion.Value.Should().Be(2);
		submittedEnvelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested.Should().BeTrue();
		var request = submittedEnvelope.Contents.Should().BeOfType<ParameterRequest>().Which;
		request.ParameterTable.Should().Be(ParameterTable.Current);
		request.ParameterNumber.Value.Should().Be(1);
		statusIdentifier.USWR.Source.Should().Be(submittedEnvelope.Source);
		statusIdentifier.USWR.Destination.Should().Be(localRouter);
		statusIdentifier.USWR.SequenceNumber.Should().Be(submittedEnvelope.AcknowledgementAndSequence.SequenceNumber);
		pendingDeliveries.IsPending(statusIdentifier).Should().BeTrue();
	}

	[Fact]
	public async Task Redirects_a_router_brigade_request_to_its_pending_status()
	{
		var statusIdentifier = new RouterParameterRequestStatusIdentifier(new UniqueSystemWideReference(
			Address(brigade: 26, node: 100, port: 25),
			Address(brigade: 26, node: 100, port: 0),
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7))));
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(statusIdentifier),
			new InMemoryPendingDeliveryRegistry());

		var result = await controller.RequestBrigadeOrAgencyNumber(CancellationToken.None);

		var redirect = result.Should().BeOfType<SeeOtherRedirectResult>().Which;
		redirect.Location.Should().Be($"/router/parameters/status/{statusIdentifier}");
		redirect.StatusCode.Should().Be(303);
	}

	[Fact]
	public async Task Keeps_a_distinct_sequence_number_for_each_pending_local_router_request()
	{
		var messageOriginator = Address(brigade: 26, node: 100, port: 25);
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		IRouterParameterRequestService service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(messageOriginator, localRouter),
			new InMemoryPendingDeliveryRegistry(),
			new RecordingRouterIngress());

		var statuses = await Task.WhenAll(
			Enumerable.Range(0, 32).Select(_ =>
				service.RequestLocalRouterBrigadeOrAgencyNumber(CancellationToken.None)));

		statuses.Select(status => status.USWR.SequenceNumber.Value).Distinct().Should().HaveCount(32);
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class RecordingRouterIngress : IRouterIngress
	{
		public Envelope? SubmittedEnvelope { get; private set; }

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.SubmittedEnvelope = envelope;
			return Task.CompletedTask;
		}
	}

	private sealed class ReturningRouterParameterRequestService(
		RouterParameterRequestStatusIdentifier statusIdentifier) : IRouterParameterRequestService
	{
		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
			CancellationToken cancellationToken)
		{
			return Task.FromResult(statusIdentifier);
		}
	}
}
