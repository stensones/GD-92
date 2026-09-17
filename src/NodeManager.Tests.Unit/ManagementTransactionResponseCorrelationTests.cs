using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit;

public sealed class ManagementTransactionResponseCorrelationTests
{
	[Fact]
	public async Task Completes_a_Parameter_Request_with_its_Parameter_Message()
	{
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(Address(25), Address(0)),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			ParameterResponse(Address(0), Address(25), identifier.USWR.SequenceNumber),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<ReceivedRouterParameterRequestStatus>().Which.ParameterValue
			.ToWireValue().Should().Equal([26]);
	}

	[Fact]
	public async Task Notifies_the_registered_browser_when_a_Parameter_Request_completes()
	{
		var notifier = new RecordingManagementTransactionUiNotifier();
		using var transactions = new TestManagementTransactions(notifier);
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(Address(25), Address(0)),
			CancellationToken.None);
		var recipient = new ManagementTransactionUiRecipient(
			"browser-session",
			Guid.ParseExact("a9c7e65f8a0b4dd5b4b0d12c9a3332d8", "N"));

		await transactions.Transactions.RegisterUiRecipientAsync(
			identifier,
			recipient,
			CancellationToken.None);
		await transactions.Transactions.ReceiveAsync(
			ParameterResponse(Address(0), Address(25), identifier.USWR.SequenceNumber),
			CancellationToken.None);

		notifier.Notifications.Should().ContainSingle().Which.Should().Be(
			new ManagementTransactionCompletion(
				"browser-session",
				recipient.RequestIdentifier,
				identifier.ToString()));
	}

	[Fact]
	public async Task Notifies_a_browser_when_it_registers_after_a_Parameter_Request_completed()
	{
		var notifier = new RecordingManagementTransactionUiNotifier();
		using var transactions = new TestManagementTransactions(notifier);
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(Address(25), Address(0)),
			CancellationToken.None);
		await transactions.Transactions.ReceiveAsync(
			ParameterResponse(Address(0), Address(25), identifier.USWR.SequenceNumber),
			CancellationToken.None);
		var recipient = new ManagementTransactionUiRecipient(
			"browser-session",
			Guid.ParseExact("0ac2e7652e824dfaab2b021d0851e6bd", "N"));

		await transactions.Transactions.RegisterUiRecipientAsync(
			identifier,
			recipient,
			CancellationToken.None);

		notifier.Notifications.Should().ContainSingle().Which.Should().Be(
			new ManagementTransactionCompletion(
				"browser-session",
				recipient.RequestIdentifier,
				identifier.ToString()));
	}

	[Fact]
	public async Task Completes_a_Node_Login_with_an_Acknowledgement()
	{
		var userAgent = Address(25);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			NodeLoginRequest(userAgent, Address(0)),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			Envelope.CreateAcknowledgement(
				Envelope.FromValues(
					userAgent,
					Destinations.FromAddresses(Address(0)),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						identifier.USWR.SequenceNumber,
						AcknowledgementRequest.Requested),
					Stensones.GD92.Messages.ParameterRequest.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(1))),
				Address(0),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<LoggedOnNodeLoginStatus>().Which.UserAgentAddress
			.Should().Be(userAgent);
	}

	[Fact]
	public async Task Records_an_invalid_password_Negative_Acknowledgement_for_a_Node_Login()
	{
		var userAgent = Address(25);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			NodeLoginRequest(userAgent, Address(0)),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			NegativeAcknowledgement(
				Address(0),
				userAgent,
				identifier.USWR.SequenceNumber,
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidPassword)),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<InvalidPasswordNodeLoginStatus>();
	}

	[Fact]
	public async Task Records_a_rejected_Negative_Acknowledgement_for_a_Node_Login()
	{
		var userAgent = Address(25);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			NodeLoginRequest(userAgent, Address(0)),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			NegativeAcknowledgement(
				Address(0),
				userAgent,
				identifier.USWR.SequenceNumber,
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax)),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<RejectedNodeLoginStatus>();
	}

	[Fact]
	public async Task Records_a_rejected_Negative_Acknowledgement_for_a_Parameter_Request()
	{
		var userAgent = Address(25);
		var router = Address(0);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(userAgent, router),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			NegativeAcknowledgement(
				router,
				userAgent,
				identifier.USWR.SequenceNumber,
				ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax)),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<RejectedRouterParameterRequestStatus>().Which.ReasonCode
			.Should().Be(ReasonCode.FromParameterReasonCode(ParameterReasonCode.InvalidSyntax));
	}

	[Fact]
	public async Task Defers_a_Parameter_Request_then_accepts_its_final_Parameter_Message()
	{
		var userAgent = Address(25);
		var router = Address(0);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(userAgent, router),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			NegativeAcknowledgement(
				router,
				userAgent,
				identifier.USWR.SequenceNumber,
				ReasonCode.FromGeneralReasonCode(GeneralReasonCode.WaitForAcknowledgement)),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<DeferredRouterParameterRequestStatus>();

		await transactions.Transactions.ReceiveAsync(
			ParameterResponse(router, userAgent, identifier.USWR.SequenceNumber),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<ReceivedRouterParameterRequestStatus>();
	}

	[Theory]
	[InlineData(true, false, false)]
	[InlineData(false, true, false)]
	[InlineData(false, false, true)]
	public async Task Leaves_a_Parameter_Request_pending_when_a_Parameter_Message_does_not_correlate(
		bool wrongSource,
		bool wrongDestination,
		bool wrongSequence)
	{
		var userAgent = Address(25);
		var router = Address(0);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(userAgent, router),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			ParameterResponse(
				wrongSource ? Address(1) : router,
				wrongDestination ? Address(26) : userAgent,
				wrongSequence
					? SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1))
					: identifier.USWR.SequenceNumber),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<PendingRouterParameterRequestStatus>();
	}

	[Fact]
	public async Task Leaves_a_Parameter_Request_pending_when_an_Acknowledgement_is_received()
	{
		var userAgent = Address(25);
		var router = Address(0);
		using var transactions = new TestManagementTransactions();
		var identifier = await transactions.Transactions.SubmitAsync(
			ParameterRequest(userAgent, router),
			CancellationToken.None);

		await transactions.Transactions.ReceiveAsync(
			Envelope.CreateAcknowledgement(
				Envelope.FromValues(
					userAgent,
					Destinations.FromAddresses(router),
					ProtocolAndPriority.FromValues(
						MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
						ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
					AcknowledgementAndSequence.FromValues(
						identifier.USWR.SequenceNumber,
						AcknowledgementRequest.Requested),
					Stensones.GD92.Messages.ParameterRequest.FromFields(
						ParameterTable.Current,
						ParameterNumber.FromValue(1))),
				router,
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			CancellationToken.None);

		transactions.Transactions.GetStatus(identifier)
			.Should().BeOfType<PendingRouterParameterRequestStatus>();
	}

	private static ManagementTransactionRequest ParameterRequest(
		CommunicationsAddress source,
		CommunicationsAddress destination) =>
		new(
			source,
			destination,
			sequenceNumber => Envelope.FromValues(
				source,
				Destinations.FromAddresses(destination),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
				AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.Requested),
				Stensones.GD92.Messages.ParameterRequest.FromFields(
					ParameterTable.Current,
					ParameterNumber.FromValue(1))),
			ManagementTransactionKind.ParameterRequest);

	private static ManagementTransactionRequest NodeLoginRequest(
		CommunicationsAddress userAgent,
		CommunicationsAddress router) =>
		new(
			userAgent,
			router,
			sequenceNumber => Envelope.FromValues(
				userAgent,
				Destinations.FromAddresses(router),
				ProtocolAndPriority.FromValues(
					MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
				AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.Requested),
				SetParameter.FromFields(
					ParameterTable.Current,
					ParameterNumber.FromValue(4),
					ParameterValue.FromWireValue([1, 2, 3]))),
			ManagementTransactionKind.NodeLogin,
			userAgent);

	private static Envelope ParameterResponse(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		SequenceNumber sequenceNumber) =>
		Envelope.FromValues(
			source,
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.NotRequested),
			Parameter.FromFields(MoreValues.No, ParameterValue.FromWireValue([26])));

	private static Envelope NegativeAcknowledgement(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		SequenceNumber sequenceNumber,
		ReasonCode reasonCode) =>
		Envelope.FromValues(
			source,
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.NotRequested),
			Stensones.GD92.Messages.NegativeAcknowledgement.FromValues(
				Destinations.FromAddresses(source),
				reasonCode));

	private static CommunicationsAddress Address(byte port) =>
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));

	private sealed class TestManagementTransactions : IDisposable
	{
		private readonly ServiceProvider serviceProvider;
		private readonly CancellationTokenSource applicationStopping = new();

		public TestManagementTransactions(IManagementTransactionUiNotifier? uiNotifier = null)
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
				NullLogger<ManagementTransactions>.Instance,
				uiNotifier);
		}

		public ManagementTransactions Transactions { get; }

		public void Dispose()
		{
			this.applicationStopping.Cancel();
			this.serviceProvider.Dispose();
			this.applicationStopping.Dispose();
		}
	}

	private sealed class RecordingManagementTransactionUiNotifier :
		IManagementTransactionUiNotifier
	{
		public List<ManagementTransactionCompletion> Notifications { get; } = [];

		public Task NotifyAsync(
			ManagementTransactionCompletion notification,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			this.Notifications.Add(notification);
			return Task.CompletedTask;
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
