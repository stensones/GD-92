using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Tests.Unit.Router.Participants;

public sealed class RouterParticipantsControllerTests
{
	[Fact]
	public void Redirects_a_new_Inventory_Scan_to_its_status()
	{
		var services = new ServiceCollection();
		services.AddScoped<IManagementTransactionService, TimedOutTransactionService>();
		using var serviceProvider = services.BuildServiceProvider();
		var inventoryScan = new InventoryScan(
			new RouterParameterRequestSettings(Address(25), Address(0)),
			InventoryScanSettings.FromConfiguration(new ConfigurationBuilder().Build()),
			serviceProvider.GetRequiredService<IServiceScopeFactory>(),
			new NonStoppingApplicationLifetime());
		var controller = new RouterParticipantsController(inventoryScan);

		var result = controller.StartDiscovery();

		result.Should().BeOfType<SeeOtherRedirectResult>().Which.Location
			.Should().MatchRegex("^/router/participants/discovery/status/[0-9a-f-]{36}$");
	}

	private static CommunicationsAddress Address(byte port) =>
		CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
			Node.FromValue(NodeIdentifier.FromValue(100)),
			Port.FromValue(PortIdentifier.FromValue(port)));

	private sealed class TimedOutTransactionService : IManagementTransactionService
	{
		public Task<RouterParameterRequestStatusIdentifier> SubmitAsync(
			ManagementTransactionRequest request,
			CancellationToken cancellationToken)
		{
			return Task.FromResult(new RouterParameterRequestStatusIdentifier(
				new UniqueSystemWideReference(
					request.Source,
					request.Destination,
					SequenceNumber.FromValue(MessageSequenceIdentifier.FromValue(1)))));
		}

		public Task<RouterParameterRequestStatus> WaitForCompletionAsync(
			RouterParameterRequestStatusIdentifier statusIdentifier,
			CancellationToken cancellationToken)
		{
			return Task.FromResult<RouterParameterRequestStatus>(
				new TimedOutRouterParameterRequestStatus(statusIdentifier));
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
