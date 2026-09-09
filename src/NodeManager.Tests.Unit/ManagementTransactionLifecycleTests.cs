using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit;

public sealed class ManagementTransactionLifecycleTests
{
	[Fact]
	public async Task Retries_a_pending_Node_Login_with_its_original_Envelope_then_times_out()
	{
		var nodeManager = Address(25);
		var router = Address(0);
		var ingress = new RecordingRouterIngress();
		var transactions = CreateTransactions(ingress, out var retryDelay);

		var identifier = await transactions.SubmitAsync(
			new ManagementTransactionRequest(
				nodeManager,
				router,
				sequenceNumber => CreateNodeLogin(nodeManager, router, sequenceNumber),
				ManagementTransactionKind.NodeLogin,
				nodeManager),
			CancellationToken.None);

		var status = await transactions.WaitForCompletionAsync(identifier, CancellationToken.None);

		ingress.SubmittedEnvelopes.Should().HaveCount(3);
		retryDelay.WaitCount.Should().Be(3);
		status.Should().BeOfType<TimedOutNodeLoginStatus>();
	}

	[Fact]
	public async Task Retains_delivery_failure_when_initial_Router_Ingress_submission_fails()
	{
		var transactions = CreateTransactions(new FailingRouterIngress(), out _);

		var identifier = await transactions.SubmitAsync(
			ParameterRequest(Address(25), Address(0)),
			CancellationToken.None);

		(await transactions.WaitForCompletionAsync(identifier, CancellationToken.None))
			.Should().BeOfType<DeliveryFailedRouterParameterRequestStatus>();
	}

	[Fact]
	public async Task Stops_retransmitting_a_deferred_Parameter_Request_then_times_it_out()
	{
		var ingress = new DeferredRouterIngress();
		var transactions = CreateTransactions(ingress, out var retryDelay);
		ingress.Transactions = transactions;

		var identifier = await transactions.SubmitAsync(
			ParameterRequest(Address(25), Address(0)),
			CancellationToken.None);

		var status = await transactions.WaitForCompletionAsync(identifier, CancellationToken.None);

		ingress.SubmittedEnvelopes.Should().ContainSingle();
		retryDelay.WaitCount.Should().Be(1);
		status.Should().BeOfType<TimedOutRouterParameterRequestStatus>();
	}

	[Fact]
	public async Task Stops_retrying_when_the_Node_Login_is_acknowledged()
	{
		var ingress = new AcknowledgingRouterIngress();
		var transactions = CreateTransactions(ingress, out _);
		ingress.Transactions = transactions;

		var identifier = await transactions.SubmitAsync(
			new ManagementTransactionRequest(
				Address(25),
				Address(0),
				sequenceNumber => CreateNodeLogin(Address(25), Address(0), sequenceNumber),
				ManagementTransactionKind.NodeLogin,
				Address(25)),
			CancellationToken.None);

		(await transactions.WaitForCompletionAsync(identifier, CancellationToken.None))
			.Should().BeOfType<LoggedOnNodeLoginStatus>();
		ingress.SubmittedEnvelopes.Should().ContainSingle();
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

	private static ManagementTransactions CreateTransactions(
		IRouterIngress ingress,
		out ImmediatelyCompletingRetryDelay retryDelay)
	{
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ => ingress);
		var serviceProvider = services.BuildServiceProvider();
		retryDelay = new ImmediatelyCompletingRetryDelay();
		return new ManagementTransactions(
			ManagementTransactionRetryPolicy.FromValues(
				ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
				ManagementTransactionTotalSends.FromValue(Word8.FromValue(3))),
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			retryDelay,
			new NonStoppingApplicationLifetime(),
			NullLogger<ManagementTransactions>.Instance);
	}

	private static Envelope CreateNodeLogin(
		CommunicationsAddress source,
		CommunicationsAddress destination,
		SequenceNumber sequenceNumber) =>
		Envelope.FromValues(
			source,
			Destinations.FromAddresses(destination),
			ProtocolAndPriority.FromValues(
				MessagePriority.FromValue(MessagePriorityLevel.FromValue(3)),
				ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2))),
			AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue([1, 2, 3])));

	private static CommunicationsAddress Address(byte port) =>
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));

	private sealed class RecordingRouterIngress : IRouterIngress
	{
		public List<Envelope> SubmittedEnvelopes { get; } = [];

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.SubmittedEnvelopes.Add(envelope);
			return Task.CompletedTask;
		}
	}

	private sealed class FailingRouterIngress : IRouterIngress
	{
		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken) =>
			Task.FromException(new InvalidOperationException("Ingress failed."));
	}

	private sealed class DeferredRouterIngress : IRouterIngress
	{
		public ManagementTransactions? Transactions { get; set; }
		public List<Envelope> SubmittedEnvelopes { get; } = [];

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.SubmittedEnvelopes.Add(envelope);
			return this.SubmittedEnvelopes.Count == 1
				? this.Transactions!.ReceiveAsync(
					Envelope.CreateNegativeAcknowledgement(
						envelope,
						envelope.Destinations.Addresses.Single(),
						envelope.ProtocolAndPriority.ProtocolVersion,
						Destinations.FromAddresses(envelope.Destinations.Addresses.Single()),
						ReasonCode.FromGeneralReasonCode(GeneralReasonCode.WaitForAcknowledgement)),
					cancellationToken)
				: Task.CompletedTask;
		}
	}

	private sealed class AcknowledgingRouterIngress : IRouterIngress
	{
		public ManagementTransactions? Transactions { get; set; }
		public List<Envelope> SubmittedEnvelopes { get; } = [];

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.SubmittedEnvelopes.Add(envelope);
			return this.Transactions!.ReceiveAsync(
				Envelope.CreateAcknowledgement(
					envelope,
					envelope.Destinations.Addresses.Single(),
					envelope.ProtocolAndPriority.ProtocolVersion),
				cancellationToken);
		}
	}

	private sealed class ImmediatelyCompletingRetryDelay : IManagementTransactionRetryDelay
	{
		public int WaitCount { get; private set; }

		public Task WaitAsync(
			ManagementTransactionNoAcknowledgementTimeout timeout,
			CancellationToken cancellationToken)
		{
			this.WaitCount++;
			return Task.CompletedTask;
		}
	}

	private sealed class NonStoppingApplicationLifetime : IHostApplicationLifetime
	{
		public CancellationToken ApplicationStarted => CancellationToken.None;
		public CancellationToken ApplicationStopping => CancellationToken.None;
		public CancellationToken ApplicationStopped => CancellationToken.None;

		public void StopApplication()
		{
		}
	}
}
