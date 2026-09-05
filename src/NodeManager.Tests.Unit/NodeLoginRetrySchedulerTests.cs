using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace NodeManager.Tests.Unit;

public sealed class NodeLoginRetrySchedulerTests
{
	[Fact]
	public async Task Schedules_only_a_Node_Login_with_the_submitted_Envelope()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var ingress = new RecordingRouterIngress();
		var retryScheduler = new RecordingRetryScheduler();
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(
				userAgent,
				router,
				NodeLoginRetryPolicy.FromValues(
					NodeLoginNoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
					NodeLoginTotalSends.FromValue(Word8.FromValue(3)))),
			new InMemoryPendingDeliveryRegistry(),
			ingress,
			retryScheduler);

		var identifier = await service.RequestLocalRouterLogon(
			userAgent,
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("FIRE1")),
			CancellationToken.None);

		retryScheduler.Identifier.Should().Be(identifier);
		retryScheduler.Envelope.Should().BeSameAs(ingress.SubmittedEnvelopes.Single());
		retryScheduler.Envelope!.AcknowledgementAndSequence.SequenceNumber.Should().Be(
			identifier.USWR.SequenceNumber);
	}

	[Fact]
	public async Task Does_not_schedule_Parameter_reads_or_Node_Logoff()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var retryScheduler = new RecordingRetryScheduler();
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(userAgent, router),
			new InMemoryPendingDeliveryRegistry(),
			new RecordingRouterIngress(),
			retryScheduler);

		await service.RequestLocalRouterBrigadeOrAgencyNumber(CancellationToken.None);
		await service.RequestLocalRouterLogoff(CancellationToken.None);

		retryScheduler.Identifier.Should().BeNull();
		retryScheduler.Envelope.Should().BeNull();
	}

	[Fact]
	public async Task Retries_a_pending_Node_Login_with_its_original_Envelope_then_times_out()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.ReserveNodeLogin(userAgent, router, userAgent);
		var initialIngress = new RecordingRouterIngress();
		var envelope = CreateNodeLogin(userAgent, router, identifier.USWR.SequenceNumber);
		await initialIngress.SubmitAsync(envelope, CancellationToken.None);
		var retryIngresses = new List<RecordingRouterIngress>();
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ =>
		{
			var ingress = new RecordingRouterIngress();
			retryIngresses.Add(ingress);
			return ingress;
		});
		await using var serviceProvider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true });
		var retryDelay = new ImmediatelyCompletingRetryDelay();
		var scheduler = new NodeLoginRetryScheduler(
			NodeLoginRetryPolicy.FromValues(
				NodeLoginNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
				NodeLoginTotalSends.FromValue(Word8.FromValue(3))),
			pendingDeliveries,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			retryDelay,
			CancellationToken.None);

		await scheduler.RetryUntilTerminalAsync(identifier, envelope);

		initialIngress.SubmittedEnvelopes.Should().ContainSingle().Which.Should().BeSameAs(envelope);
		retryIngresses.Should().HaveCount(2);
		retryIngresses.Should().AllSatisfy(ingress =>
			ingress.SubmittedEnvelopes.Should().ContainSingle().Which.Should().BeSameAs(envelope));
		retryDelay.WaitCount.Should().Be(3);
		pendingDeliveries.IsPending(identifier).Should().BeFalse();
		pendingDeliveries.GetStatus(identifier).Should().BeOfType<TimedOutNodeLoginStatus>();
	}

	[Fact]
	public async Task Stops_retrying_when_the_Node_Login_is_acknowledged()
	{
		var userAgent = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var identifier = pendingDeliveries.ReserveNodeLogin(userAgent, router, userAgent);
		var ingress = new RecordingRouterIngress();
		var envelope = CreateNodeLogin(userAgent, router, identifier.USWR.SequenceNumber);
		await ingress.SubmitAsync(envelope, CancellationToken.None);
		var retryDelay = new CompletingRetryDelay(() =>
			pendingDeliveries.TryCompleteAcknowledgement(
				Envelope.CreateAcknowledgement(
					envelope,
					router,
					ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2)))));
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ => ingress);
		await using var serviceProvider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true });
		var scheduler = new NodeLoginRetryScheduler(
			NodeLoginRetryPolicy.FromValues(
				NodeLoginNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
				NodeLoginTotalSends.FromValue(Word8.FromValue(3))),
			pendingDeliveries,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			retryDelay,
			CancellationToken.None);

		await scheduler.RetryUntilTerminalAsync(identifier, envelope);

		ingress.SubmittedEnvelopes.Should().ContainSingle().Which.Should().BeSameAs(envelope);
		retryDelay.WaitCount.Should().Be(1);
		pendingDeliveries.GetStatus(identifier).Should().BeOfType<LoggedOnNodeLoginStatus>();
	}

	private static Envelope CreateNodeLogin(
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
			AcknowledgementAndSequence.FromValues(sequenceNumber, AcknowledgementRequest.Requested),
			SetParameter.FromFields(
				ParameterTable.Current,
				ParameterNumber.FromValue(4),
				ParameterValue.FromWireValue([1, 2, 3])));
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
		public List<Envelope> SubmittedEnvelopes { get; } = [];

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			this.SubmittedEnvelopes.Add(envelope);
			return Task.CompletedTask;
		}
	}

	private sealed class ImmediatelyCompletingRetryDelay : INodeLoginRetryDelay
	{
		public int WaitCount { get; private set; }

		public Task WaitAsync(
			NodeLoginNoAcknowledgementTimeout timeout,
			CancellationToken cancellationToken)
		{
			this.WaitCount++;
			return Task.CompletedTask;
		}
	}

	private sealed class RecordingRetryScheduler : INodeLoginRetryScheduler
	{
		public RouterParameterRequestStatusIdentifier? Identifier { get; private set; }
		public Envelope? Envelope { get; private set; }

		public void Schedule(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			Envelope envelope)
		{
			this.Identifier = statusIdentifier;
			this.Envelope = envelope;
		}
	}

	private sealed class CompletingRetryDelay(Func<bool> complete) : INodeLoginRetryDelay
	{
		public int WaitCount { get; private set; }

		public Task WaitAsync(
			NodeLoginNoAcknowledgementTimeout timeout,
			CancellationToken cancellationToken)
		{
			this.WaitCount++;
			complete().Should().BeTrue();
			return Task.CompletedTask;
		}
	}
}
