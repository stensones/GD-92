using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit;

public sealed class RouterParametersControllerTests
{
	[Fact]
	public void Reads_statuses_through_the_concrete_Management_Transaction_Module()
	{
		var constructor = typeof(RouterParametersController).GetConstructors().Single();

		constructor.GetParameters().Select(parameter => parameter.ParameterType.Name)
			.Should().BeEquivalentTo(
				["IRouterParameterRequestService", "ManagementTransactions"]);
	}

	[Fact]
	public async Task Redirects_a_local_router_brigade_request_to_its_transaction_status()
	{
		var identifier = Identifier();
		using var managementTransactions = new TestManagementTransactions();
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var result = await controller.RequestBrigadeOrAgencyNumber(CancellationToken.None);

		result.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/status/{identifier}");
	}

	[Fact]
	public async Task Redirects_logon_and_logoff_to_compatible_status_routes()
	{
		var identifier = Identifier();
		using var managementTransactions = new TestManagementTransactions();
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var logon = await controller.LogOn("FIRE1", 26, 100, 25, CancellationToken.None);
		var logoff = await controller.LogOff(CancellationToken.None);

		logon.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/logon/status/{identifier}");
		logoff.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/logoff/status/{identifier}");
	}

	[Fact]
	public async Task Presents_a_received_Parameter_Response()
	{
		using var managementTransactions = new TestManagementTransactions();
		var identifier = await managementTransactions.Transactions.SubmitAsync(
			new ManagementTransactionRequest(
				Address(25),
				Address(0),
				sequenceNumber => Envelope.FromValues(
					Address(25),
					Destinations.FromAddresses(Address(0)),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						sequenceNumber,
						AcknowledgementRequest.Requested),
					ParameterRequest.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(1))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26]))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.Status(identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.BrigadeOrAgencyNumber.Should().Be(26);
	}

	[Fact]
	public void Returns_not_found_for_an_unknown_or_invalid_status()
	{
		using var managementTransactions = new TestManagementTransactions();
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(Identifier()),
			managementTransactions.Transactions);

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

	private sealed class TestManagementTransactions : IDisposable
	{
		private readonly ServiceProvider serviceProvider;
		private readonly CancellationTokenSource applicationStopping = new();

		public TestManagementTransactions()
		{
			var services = new ServiceCollection();
			services.AddScoped<IRouterIngress, NoOpRouterIngress>();
			this.serviceProvider = services.BuildServiceProvider();
			this.Transactions = new ManagementTransactions(
				ManagementTransactionRetryPolicy.FromValues(
					ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
					ManagementTransactionTotalSends.FromValue(Word8.FromValue(1))),
				this.serviceProvider.GetRequiredService<IServiceScopeFactory>(),
				new BlockingRetryDelay(),
				new TestApplicationLifetime(this.applicationStopping.Token),
				NullLogger<ManagementTransactions>.Instance);
		}

		public ManagementTransactions Transactions { get; }

		public void Dispose()
		{
			this.applicationStopping.Cancel();
			this.serviceProvider.Dispose();
			this.applicationStopping.Dispose();
		}
	}

	private sealed class NoOpRouterIngress : IRouterIngress
	{
		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}

	private sealed class BlockingRetryDelay : IManagementTransactionRetryDelay
	{
		public Task WaitAsync(
			ManagementTransactionNoAcknowledgementTimeout timeout,
			CancellationToken cancellationToken) =>
			Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
	}

	private sealed class TestApplicationLifetime(CancellationToken applicationStopping) :
		IHostApplicationLifetime
	{
		public CancellationToken ApplicationStarted => CancellationToken.None;
		public CancellationToken ApplicationStopping => applicationStopping;
		public CancellationToken ApplicationStopped => CancellationToken.None;

		public void StopApplication()
		{
		}
	}
}
