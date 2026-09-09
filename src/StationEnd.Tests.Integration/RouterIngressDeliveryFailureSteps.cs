using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NodeManager.Router.Parameters;
using Reqnroll;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class RouterIngressDeliveryFailureSteps
{
	private RouterParametersController? controller;
	private IActionResult? result;
	private string? statusIdentifier;

	[Given(@"NodeManager's configured Router Ingress cannot submit a management Envelope")]
	public void GivenNodeManagersConfiguredRouterIngressCannotSubmitAManagementEnvelope()
	{
		var nodeManager = Address(brigade: 26, node: 100, port: 25);
		var router = Address(brigade: 26, node: 100, port: 0);
		var ingress = new FailingRouterIngress();
		var services = new ServiceCollection();
		services.AddScoped<IRouterIngress>(_ => ingress);
		var serviceProvider = services.BuildServiceProvider();
		var managementTransactions = new ManagementTransactions(
			RouterParameterRequestSettings.DefaultManagementTransactionRetryPolicy,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new ManagementTransactionRetryDelay(),
			new NonStoppingApplicationLifetime(),
			NullLogger<ManagementTransactions>.Instance);
		var service = new RouterParameterRequestService(
			new RouterParameterRequestSettings(nodeManager, router),
			managementTransactions);

		this.controller = new RouterParametersController(service, managementTransactions);
	}

	[When(@"the NodeManager user requests the local Router brigade or agency number")]
	public async Task WhenTheNodeManagerUserRequestsTheLocalRouterBrigadeOrAgencyNumber()
	{
		this.result = await this.controller!.RequestBrigadeOrAgencyNumber(CancellationToken.None);
	}

	[Then(@"NodeManager redirects to the retained Parameter Request status")]
	public void ThenNodeManagerRedirectsToTheRetainedParameterRequestStatus()
	{
		var redirect = this.result.Should().BeOfType<SeeOtherRedirectResult>().Which;
		redirect.Location.Should().StartWith("/router/parameters/status/");
		this.statusIdentifier = redirect.Location!["/router/parameters/status/".Length..];
	}

	[Then(@"the Parameter Request status shows delivery-failed")]
	public void ThenTheParameterRequestStatusShowsDeliveryFailed()
	{
		var response = this.controller!.Status(this.statusIdentifier!)
			.Should().BeOfType<OkObjectResult>().Which.Value
			.Should().BeOfType<RouterParameterRequestStatusResponse>().Which;

		response.State.Should().Be("delivery-failed");
	}

	private static CommunicationsAddress Address(byte brigade, ushort node, byte port)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(brigade)),
			Node.FromValue(NodeIdentifier.FromValue(node)),
			Port.FromValue(PortIdentifier.FromValue(port)));
	}

	private sealed class FailingRouterIngress : IRouterIngress
	{
		public Task SubmitAsync(Envelope envelope, CancellationToken cancellationToken)
		{
			return Task.FromException(
				new InvalidOperationException("The Router Ingress could not submit the management Envelope."));
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
