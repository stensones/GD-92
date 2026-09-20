using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class NodeManagerNetworkWorkspaceSteps : IDisposable
{
	private NodeManagerApplicationFactory? application;
	private HttpResponseMessage? response;
	private string? responseContent;

	[Given(@"an operator opens the Network workspace for local Router address (.*)")]
	public void GivenAnOperatorOpensTheNetworkWorkspaceForLocalRouterAddress(string address)
	{
		this.application = new NodeManagerApplicationFactory();
		this.response = this.application.CreateClient().GetAsync($"/network?address={address}")
			.GetAwaiter()
			.GetResult();
	}

	[When(@"the Network workspace is displayed")]
	public async Task WhenTheNetworkWorkspaceIsDisplayed()
	{
		this.response.Should().NotBeNull();
		this.response!.IsSuccessStatusCode.Should().BeTrue(
			because: "the Network workspace is an operator-facing route");
		this.responseContent = await this.response.Content.ReadAsStringAsync();
	}

	[Then(@"the selected Communications Address (.*) is visible")]
	public void ThenTheSelectedCommunicationsAddressIsVisible(string address)
	{
		this.responseContent.Should().Contain(address);
	}

	[Then(@"local Participant discovery is available")]
	public void ThenLocalParticipantDiscoveryIsAvailable()
	{
		this.responseContent.Should().Contain("Inventory Scan");
	}

	[Then(@"unsupported remote catalogue discovery is identified as unavailable")]
	public void ThenUnsupportedRemoteCatalogueDiscoveryIsIdentifiedAsUnavailable()
	{
		this.responseContent.Should().Contain("Remote catalogue discovery is unavailable");
	}

	[Then(@"Inventory Scan progress is announced in a labelled status region")]
	public void ThenInventoryScanProgressIsAnnouncedInALabelledStatusRegion()
	{
		this.responseContent.Should().Contain("id=\"network-inventory-scan-status\"");
		this.responseContent.Should().Contain("role=\"status\"");
	}

	[Then(@"discovered Participants are presented in a results table")]
	public void ThenDiscoveredParticipantsArePresentedInAResultsTable()
	{
		this.responseContent.Should().Contain("<caption>Discovered local Participants</caption>");
		this.responseContent.Should().Contain("<th scope=\"col\">Port</th>");
		this.responseContent.Should().Contain("<th scope=\"col\">Agent Type</th>");
	}

	[Then(@"Inventory Scan outcomes identify received, rejected, timed out, and delivery failed states")]
	public void ThenInventoryScanOutcomesIdentifyTerminalStates()
	{
		this.responseContent.Should().Contain("Received");
		this.responseContent.Should().Contain("Rejected");
		this.responseContent.Should().Contain("Timed out");
		this.responseContent.Should().Contain("Delivery failed");
	}

	public void Dispose()
	{
		this.response?.Dispose();
		this.application?.Dispose();
	}

	private sealed class NodeManagerApplicationFactory : WebApplicationFactory<NodeManagerApplication>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Testing");
		}
	}
}
