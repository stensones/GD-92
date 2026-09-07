using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NodeManager.Router.Parameters;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class DeferredParameterRequestSteps
{
	private ServiceProvider? serviceProvider;
	private RouterParametersController? controller;
	private DeferredRouterIngress? routerIngress;
	private RouterParameterRequestStatusIdentifier? statusIdentifier;

	[Given(@"the local Router responds to a Parameter Request with wait_ack and no final response")]
	public void GivenTheLocalRouterRespondsWithWaitAckAndNoFinalResponse()
	{
		var nodeManager = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var pendingDeliveries = new InMemoryPendingDeliveryRegistry();
		var responses = new RouterParameterResponseReceiver(pendingDeliveries);
		this.routerIngress = new DeferredRouterIngress(responses);
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ => this.routerIngress);
		this.serviceProvider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateScopes = true });
		var scheduler = new NodeLoginRetryScheduler(
			NodeLoginRetryPolicy.FromValues(
				NodeLoginNoAcknowledgementTimeout.FromValue(Word8.FromValue(1)),
				NodeLoginTotalSends.FromValue(Word8.FromValue(3))),
			pendingDeliveries,
			this.serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new ImmediatelyCompletingRetryDelay(),
			CancellationToken.None);
		var requests = new RouterParameterRequestService(
			new RouterParameterRequestSettings(nodeManager, router),
			pendingDeliveries,
			this.routerIngress,
			scheduler);
		this.controller = new RouterParametersController(requests, pendingDeliveries);
	}

	[When(@"an awaiting NodeManager user requests the local Router brigade or agency number")]
	public async Task WhenTheNodeManagerUserRequestsTheLocalRouterBrigadeOrAgencyNumber()
	{
		var result = await this.controller!.RequestBrigadeOrAgencyNumber(CancellationToken.None);
		var redirect = result.Should().BeOfType<SeeOtherRedirectResult>().Which;
		this.statusIdentifier = RouterParameterRequestStatusIdentifier.TryParse(
			redirect.Location!["/router/parameters/status/".Length..],
			out var identifier)
			? identifier
			: throw new Xunit.Sdk.XunitException("NodeManager did not redirect to a Parameter Request status.");
	}

	[Then(@"NodeManager submits the Parameter Request only once")]
	public void ThenNodeManagerSubmitsTheParameterRequestOnlyOnce()
	{
		this.routerIngress!.SubmitCount.Should().Be(1);
	}

	[Then(@"the deferred Parameter Request status eventually shows timed-out")]
	public async Task ThenTheDeferredParameterRequestStatusEventuallyShowsTimedOut()
	{
		for (var attempt = 0; attempt < 30; attempt++)
		{
			var response = this.controller!.Status(this.statusIdentifier!.ToString())
				.Should().BeOfType<OkObjectResult>().Which.Value
				.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;
			if (response.State == "timed-out")
			{
				return;
			}

			await Task.Delay(TimeSpan.FromMilliseconds(100));
		}

		throw new Xunit.Sdk.XunitException(
			"The deferred Parameter Request did not become timed-out.");
	}

	[AfterScenario]
	public void DisposeServiceProvider()
	{
		this.serviceProvider?.Dispose();
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class DeferredRouterIngress(
		RouterParameterResponseReceiver responses) : IRouterIngress
	{
		private int submitCount;

		public int SubmitCount => this.submitCount;

		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			if (Interlocked.Increment(ref this.submitCount) != 1)
			{
				return Task.CompletedTask;
			}

			return responses.ReceiveAsync(
				Envelope.CreateNegativeAcknowledgement(
					envelope,
					envelope.Destinations.Addresses.Single(),
					envelope.ProtocolAndPriority.ProtocolVersion,
					Destinations.FromAddresses(envelope.Destinations.Addresses.Single()),
					ReasonCode.FromGeneralReasonCode(GeneralReasonCode.WaitForAcknowledgement)),
				cancellationToken);
		}
	}

	private sealed class ImmediatelyCompletingRetryDelay : INodeLoginRetryDelay
	{
		public Task WaitAsync(
			NodeLoginNoAcknowledgementTimeout timeout,
			CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
