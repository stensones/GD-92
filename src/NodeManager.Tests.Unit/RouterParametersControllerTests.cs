using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using System.Text.Json;

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
			ingress,
			NoOpNodeLoginRetryScheduler.Instance);

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
	public async Task Retains_a_terminal_status_when_initial_Router_Ingress_submission_fails()
	{
		var messageOriginator = Address(brigade: 26, node: 100, port: 25);
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		IRouterParameterRequestService service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(messageOriginator, localRouter),
			pendingDeliveries,
			new FailingRouterIngress(),
			NoOpNodeLoginRetryScheduler.Instance);

		var statusIdentifier = await service.RequestLocalRouterBrigadeOrAgencyNumber(
			CancellationToken.None);

		pendingDeliveries.IsPending(statusIdentifier).Should().BeFalse();
		pendingDeliveries.GetStatus(statusIdentifier).Should().NotBeNull();
	}

	[Fact]
	public async Task Submits_a_level_one_logon_to_the_local_Router_and_returns_its_pending_USWR()
	{
		var messageOriginator = Address(brigade: 26, node: 100, port: 25);
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var suppliedUserAgent = Address(brigade: 26, node: 100, port: 25);
		var ingress = new RecordingRouterIngress();
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(messageOriginator, localRouter),
			pendingDeliveries,
			ingress,
			NoOpNodeLoginRetryScheduler.Instance);

		var statusIdentifier = await service.RequestLocalRouterLogon(
			suppliedUserAgent,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1")),
			CancellationToken.None);

		ingress.SubmittedEnvelope.Should().NotBeNull();
		var submittedEnvelope = ingress.SubmittedEnvelope!;
		submittedEnvelope.Source.Should().Be(messageOriginator);
		submittedEnvelope.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(localRouter);
		submittedEnvelope.ProtocolAndPriority.Priority.Value.Should().Be(3);
		submittedEnvelope.ProtocolAndPriority.ProtocolVersion.Value.Should().Be(2);
		submittedEnvelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested.Should().BeTrue();
		var logon = submittedEnvelope.Contents.Should().BeOfType<SetParameter>().Which;
		logon.ParameterTable.Should().Be(ParameterTable.Current);
		logon.ParameterNumber.Value.Should().Be(4);
		var passwordBuffer = new EncodedMessageBuffer(logon.ParameterValue.ToWireValue());
		var passwordParameter = PasswordParameter.FromEncodedMessageBuffer(ref passwordBuffer);
		passwordParameter.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Level1));
		passwordParameter.CommunicationsAddress.Should().Be(suppliedUserAgent);
		passwordParameter.Password.Value.Should().Be(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1")));
		statusIdentifier.USWR.Source.Should().Be(messageOriginator);
		statusIdentifier.USWR.Destination.Should().Be(localRouter);
		statusIdentifier.USWR.SequenceNumber.Should().Be(submittedEnvelope.AcknowledgementAndSequence.SequenceNumber);
		pendingDeliveries.IsPending(statusIdentifier).Should().BeTrue();
	}

	[Fact]
	public async Task Submits_a_level_zero_logoff_from_the_NodeManager_address_to_the_local_Router()
	{
		var messageOriginator = Address(brigade: 26, node: 100, port: 25);
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		var ingress = new RecordingRouterIngress();
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(messageOriginator, localRouter),
			pendingDeliveries,
			ingress,
			NoOpNodeLoginRetryScheduler.Instance);

		var statusIdentifier = await service.RequestLocalRouterLogoff(CancellationToken.None);

		ingress.SubmittedEnvelope.Should().NotBeNull();
		var submittedEnvelope = ingress.SubmittedEnvelope!;
		submittedEnvelope.Source.Should().Be(messageOriginator);
		submittedEnvelope.Destinations.Addresses.Should().ContainSingle().Which.Should().Be(localRouter);
		var logoff = submittedEnvelope.Contents.Should().BeOfType<SetParameter>().Which;
		logoff.ParameterTable.Should().Be(ParameterTable.Current);
		logoff.ParameterNumber.Value.Should().Be(4);
		var passwordBuffer = new EncodedMessageBuffer(logoff.ParameterValue.ToWireValue());
		var passwordParameter = PasswordParameter.FromEncodedMessageBuffer(ref passwordBuffer);
		passwordParameter.Level.Should().Be(
			PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated));
		passwordParameter.Password.Value.Value.Value.Should().BeEmpty();
		passwordParameter.CommunicationsAddress.Should().Be(messageOriginator);
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
	public async Task Redirects_a_local_Router_logon_to_its_pending_Node_Login_status()
	{
		var statusIdentifier = new RouterParameterRequestStatusIdentifier(new UniqueSystemWideReference(
			Address(brigade: 26, node: 100, port: 25),
			Address(brigade: 26, node: 100, port: 0),
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7))));
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(statusIdentifier),
			new InMemoryPendingDeliveryRegistry());

		var result = await controller.LogOn("FIRE1", 26, 100, 25, CancellationToken.None);

		var redirect = result.Should().BeOfType<SeeOtherRedirectResult>().Which;
		redirect.Location.Should().Be($"/router/parameters/logon/status/{statusIdentifier}");
		redirect.StatusCode.Should().Be(303);
	}

	[Fact]
	public async Task Redirects_an_HTTPS_local_Router_logoff_to_its_pending_Node_Login_status()
	{
		var statusIdentifier = new RouterParameterRequestStatusIdentifier(new UniqueSystemWideReference(
			Address(brigade: 26, node: 100, port: 25),
			Address(brigade: 26, node: 100, port: 0),
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7))));
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(statusIdentifier),
			new InMemoryPendingDeliveryRegistry());

		var result = await controller.LogOff(CancellationToken.None);

		var redirect = result.Should().BeOfType<SeeOtherRedirectResult>().Which;
		redirect.Location.Should().Be($"/router/parameters/logoff/status/{statusIdentifier}");
		redirect.StatusCode.Should().Be(303);
		var action = typeof(RouterParametersController)
			.GetMethod(nameof(RouterParametersController.LogOff))!;
		action.GetCustomAttributes(inherit: true)
			.OfType<RequireHttpsAttribute>()
			.Should().ContainSingle();
	}

	[Fact]
	public void Rejects_a_non_HTTPS_logon_before_the_action_can_process_its_form()
	{
		var action = typeof(RouterParametersController)
			.GetMethod(nameof(RouterParametersController.LogOn))!;
		var httpsRequirement = action.GetCustomAttributes(inherit: true)
			.OfType<RequireHttpsAttribute>()
			.Should().ContainSingle().Which;
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Method = HttpMethods.Post;
		httpContext.Request.Scheme = Uri.UriSchemeHttp;
		var filterContext = new AuthorizationFilterContext(
			new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
			[]);

		httpsRequirement.OnAuthorization(filterContext);

		filterContext.Result.Should().NotBeNull();
	}

	[Fact]
	public async Task Keeps_a_distinct_sequence_number_for_each_pending_local_router_request()
	{
		var messageOriginator = Address(brigade: 26, node: 100, port: 25);
		var localRouter = Address(brigade: 26, node: 100, port: 0);
		IRouterParameterRequestService service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(messageOriginator, localRouter),
			new InMemoryPendingDeliveryRegistry(),
			new RecordingRouterIngress(),
			NoOpNodeLoginRetryScheduler.Instance);

		var statuses = await Task.WhenAll(
			Enumerable.Range(0, 32).Select(_ =>
				service.RequestLocalRouterBrigadeOrAgencyNumber(CancellationToken.None)));

		statuses.Select(status => status.USWR.SequenceNumber.Value).Distinct().Should().HaveCount(32);
	}

	[Fact]
	public void Returns_a_public_received_status_with_the_local_Router_brigade_number()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.Reserve(userAgent, router);
		pendingDeliveries.TryCompleteParameterResponse(Envelope.FromValues(
			router,
			Destinations.FromAddresses(userAgent),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				identifier.USWR.SequenceNumber,
				AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26]))));
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			pendingDeliveries);

		var result = controller.Status(identifier.ToString());

		var response = result.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;
		response.State.Should().Be("received");
		response.BrigadeOrAgencyNumber.Should().Be(26);
	}

	[Fact]
	public async Task Returns_a_public_logged_on_status_without_the_submitted_password()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var ingress = new RecordingRouterIngress();
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(userAgent, router),
			pendingDeliveries,
			ingress,
			NoOpNodeLoginRetryScheduler.Instance);
		var identifier = await service.RequestLocalRouterLogon(
			userAgent,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1")),
			CancellationToken.None);
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
		var controller = new RouterParametersController(service, pendingDeliveries);

		var result = controller.Status(identifier.ToString());

		var response = result.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;
		response.State.Should().Be("logged-on");
		response.UserAgentAddress.Should().Be("26.100.25");
		JsonSerializer.Serialize(response).Should().NotContain("FIRE1");
	}

	[Fact]
	public async Task Returns_a_public_logged_off_status_after_correlating_the_local_Router_ACK()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var ingress = new RecordingRouterIngress();
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(userAgent, router),
			pendingDeliveries,
			ingress,
			NoOpNodeLoginRetryScheduler.Instance);
		var identifier = await service.RequestLocalRouterLogoff(CancellationToken.None);
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
		var controller = new RouterParametersController(service, pendingDeliveries);

		var result = controller.Status(identifier.ToString());

		var response = result.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;
		response.State.Should().Be("logged-off");
		JsonSerializer.Serialize(response).Should().Contain("logged-off");
	}

	[Fact]
	public async Task Returns_an_invalid_password_status_after_the_User_Agent_receiver_correlates_the_local_Router_NAK()
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
				NegativeAcknowledgement.FromValues(
					Destinations.FromAddresses(router),
					ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidPassword))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			pendingDeliveries);

		var result = controller.Status(identifier.ToString());

		var response = result.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;
		response.State.Should().Be("invalid_password");
		JsonSerializer.Serialize(response).Should().Contain("invalid_password");
	}

	[Fact]
	public void Returns_a_timed_out_status_after_a_Node_Login_exhausts_its_sends()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.ReserveNodeLogin(userAgent, router, userAgent);
		pendingDeliveries.TryTimeoutNodeLogin(identifier).Should().BeTrue();
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			pendingDeliveries);

		var result = controller.Status(identifier.ToString());

		var response = result.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;
		response.State.Should().Be("timed-out");
		response.UserAgentAddress.Should().Be("26.100.25");
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

	private sealed class FailingRouterIngress : IRouterIngress
	{
		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			return Task.FromException(
				new InvalidOperationException("The Router Ingress could not submit the management Envelope."));
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

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
			CommunicationsAddress communicationsAddress,
			PasswordValue password,
			CancellationToken cancellationToken)
		{
			return Task.FromResult(statusIdentifier);
		}

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogoff(
			CancellationToken cancellationToken)
		{
			return Task.FromResult(statusIdentifier);
		}
	}

	private sealed class NoOpNodeLoginRetryScheduler : INodeLoginRetryScheduler
	{
		public static NoOpNodeLoginRetryScheduler Instance { get; } = new();

		public void Schedule(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			Envelope envelope)
		{
		}
	}
}
