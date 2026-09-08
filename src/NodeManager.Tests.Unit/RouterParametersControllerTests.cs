using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Tests.Unit;

public sealed class RouterParametersControllerTests
{
	[Fact]
	public void Reads_statuses_through_a_narrow_Management_Transaction_Interface()
	{
		var constructor = typeof(RouterParametersController).GetConstructors().Single();

		constructor.GetParameters().Select(parameter => parameter.ParameterType.Name)
			.Should().BeEquivalentTo(
				["IRouterParameterRequestService", "IManagementTransactionStatusReader"]);
	}

	[Fact]
	public async Task Redirects_a_local_router_brigade_request_to_its_transaction_status()
	{
		var identifier = Identifier();
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			new InMemoryManagementTransactionRegistry());

		var result = await controller.RequestBrigadeOrAgencyNumber(CancellationToken.None);

		result.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/status/{identifier}");
	}

	[Fact]
	public async Task Redirects_logon_and_logoff_to_compatible_status_routes()
	{
		var identifier = Identifier();
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			new InMemoryManagementTransactionRegistry());

		var logon = await controller.LogOn("FIRE1", 26, 100, 25, CancellationToken.None);
		var logoff = await controller.LogOff(CancellationToken.None);

		logon.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/logon/status/{identifier}");
		logoff.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/logoff/status/{identifier}");
	}

	[Fact]
	public void Presents_a_received_Parameter_Response()
	{
		var registry = new InMemoryManagementTransactionRegistry();
		var identifier = registry.ReserveParameterRequest(Address(25), Address(0));
		registry.TryCompleteParameterResponse(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26]))));
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			registry);

		var response = controller.Status(identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.BrigadeOrAgencyNumber.Should().Be(26);
	}

	[Fact]
	public void Returns_not_found_for_an_unknown_or_invalid_status()
	{
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(Identifier()),
			new InMemoryManagementTransactionRegistry());

		controller.Status("not-a-transaction").Should().BeOfType<NotFoundResult>();
		controller.Status(Identifier().ToString()).Should().BeOfType<NotFoundResult>();
	}

	private static RouterParameterRequestStatusIdentifier Identifier() =>
		new(new UniqueSystemWideReference(
			Address(25),
			Address(0),
			SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(7))));

	private static CommunicationsAddress Address(byte port) =>
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));

	private sealed class ReturningRouterParameterRequestService(
		RouterParameterRequestStatusIdentifier statusIdentifier) : IRouterParameterRequestService
	{
		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
			CancellationToken cancellationToken) => Task.FromResult(statusIdentifier);

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogon(
			CommunicationsAddress communicationsAddress,
			PasswordValue password,
			CancellationToken cancellationToken) => Task.FromResult(statusIdentifier);

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterLogoff(
			CancellationToken cancellationToken) => Task.FromResult(statusIdentifier);
	}
}
