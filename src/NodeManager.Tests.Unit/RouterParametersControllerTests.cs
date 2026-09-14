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
	public async Task Redirects_a_selected_Current_Router_Parameter_to_its_parameter_status()
	{
		var identifier = Identifier();
		using var managementTransactions = new TestManagementTransactions();
		var requests = new ReturningRouterParameterRequestService(identifier);
		var controller = new RouterParametersController(
			requests,
			managementTransactions.Transactions);

		var result = await controller.RequestCurrentParameter(1, CancellationToken.None);

		requests.CurrentParameterNumber.Should().Be(ParameterNumber.FromValue(1));
		result.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().Be($"/router/parameters/current/1/status/{identifier}");
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
		response.MoreValues.Should().BeFalse();
	}

	[Fact]
	public async Task Presents_a_parameter_Invalid_Entry_rejection_reason()
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
					ParameterRequestMultiple.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(13),
						ParameterEntrySelection.Range(
							ParameterEntryIndex.FromValue(2),
							ParameterEntryIndex.FromValue(2)))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(
				identifier.USWR.SequenceNumber,
				AcknowledgementRequest.NotRequested),
			NegativeAcknowledgement.FromValues(
				Destinations.FromAddresses(Address(0)),
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidEntry))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.Status(identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("rejected");
		response.RejectionReason.Should().Be("Parameter / Invalid Entry");
	}

	[Fact]
	public async Task Presents_a_received_Current_Password_without_exposing_its_secret()
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
						ParameterNumber.FromValue(4))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(
					PasswordParameter.FromFields(
						PasswordLevel.FromValue(PasswordLevelNumber.Unauthenticated),
						Password.FromValue(
							PasswordValue.FromValue(SevenBitAsciiString.FromValue("PASSWORD"))),
						Address(0)).ToWireValue()))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(4, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(4);
		response.ParameterValue.Should().Be("Level 0, User-Agent 26.100.0, PASSWORD");
	}

	[Fact]
	public async Task Presents_a_received_Current_Node_Name()
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
						ParameterNumber.FromValue(3))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(
					NodeName.FromValue(SevenBitAsciiString.FromValue("Station End")).ToWireValue()))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(3, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(3);
		response.ParameterValue.Should().Be("Station End");
	}

	[Fact]
	public async Task Presents_a_received_Current_Maximum_Message_Length()
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
						ParameterNumber.FromValue(9))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(MaximumMessageLength.FromValue(1023).ToWireValue()))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(9, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(9);
		response.ParameterValue.Should().Be("1023");
	}

	[Fact]
	public async Task Presents_a_received_Current_Network_Manager_Address_1()
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
						ParameterNumber.FromValue(10))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(Address(25).ToWireValue()))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(10, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(10);
		response.ParameterValue.Should().Be("26.100.25");
	}

	[Fact]
	public async Task Presents_a_received_Level1_Password_as_a_redacted_marker()
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
						ParameterNumber.FromValue(5))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(
				MoreValues.No,
				ParameterValue.FromWireValue(
					Password.FromValue(
						PasswordValue.FromValue(SevenBitAsciiString.FromValue("PASSWORD"))).ToWireValue()))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(5, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(5);
		response.ParameterValue.Should().Be("PASSWORD");
	}

	[Fact]
	public async Task Presents_a_received_Current_No_Acknowledgement_Timeout()
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
						ParameterNumber.FromValue(12))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([5]))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(12, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(12);
		response.ParameterValue.Should().Be("5");
	}

	[Fact]
	public async Task Presents_a_received_Current_Retries()
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
						ParameterNumber.FromValue(19))),
				ManagementTransactionKind.ParameterRequest),
			CancellationToken.None);
		await managementTransactions.Transactions.ReceiveAsync(Envelope.FromValues(
			Address(0),
			Destinations.FromAddresses(Address(25)),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(identifier.USWR.SequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([3]))),
			CancellationToken.None);
		var controller = new RouterParametersController(
			new ReturningRouterParameterRequestService(identifier),
			managementTransactions.Transactions);

		var response = controller.CurrentStatus(19, identifier.ToString())
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("received");
		response.ParameterNumber.Should().Be(19);
		response.ParameterValue.Should().Be("3");
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
		public ParameterNumber? CurrentParameterNumber { get; private set; }

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterBrigadeOrAgencyNumber(
			CancellationToken cancellationToken) => Task.FromResult(statusIdentifier);

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterCurrentParameter(
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken)
		{
			this.CurrentParameterNumber = parameterNumber;
			return Task.FromResult(statusIdentifier);
		}

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterParameter(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			CancellationToken cancellationToken) => Task.FromResult(statusIdentifier);

		public Task<RouterParameterRequestStatusIdentifier> RequestLocalRouterParameterEntries(
			ParameterTable parameterTable,
			ParameterNumber parameterNumber,
			ParameterEntrySelection entrySelection,
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
