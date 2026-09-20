using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class NodeManagerParametersWorkspaceSteps : IDisposable
{
	private NodeManagerApplicationFactory? application;
	private HttpResponseMessage? response;
	private string? responseContent;

	[Given(@"an operator opens the default Parameters workspace for Router address (.*)")]
	public void GivenAnOperatorOpensTheParametersWorkspaceForRouterAddress(string address)
	{
		this.application = new NodeManagerApplicationFactory();
		this.response = this.application.CreateClient()
			.GetAsync($"/parameters?address={address}&agentType=Router")
			.GetAwaiter()
			.GetResult();
	}

	[Given(@"an operator opens the Parameters workspace for Router address (.*) with the Permanent Parameter Table")]
	public void GivenAnOperatorOpensTheParametersWorkspaceWithThePermanentParameterTable(
		string address)
	{
		this.application = new NodeManagerApplicationFactory();
		this.response = this.application.CreateClient()
			.GetAsync($"/parameters?address={address}&agentType=Router&parameterTable=permanent")
			.GetAwaiter()
			.GetResult();
	}

	[Given(@"an operator opens the Parameters workspace for LAN MTA \(10\) address (.*)")]
	public void GivenAnOperatorOpensTheParametersWorkspaceForLanMta(string address)
	{
		this.application = new NodeManagerApplicationFactory();
		this.response = this.application.CreateClient()
			.GetAsync($"/parameters?address={address}&agentType=LAN%20MTA%20%2810%29")
			.GetAwaiter()
			.GetResult();
	}

	[When(@"the Parameters workspace is displayed")]
	public async Task WhenTheParametersWorkspaceIsDisplayed()
	{
		this.response.Should().NotBeNull();
		this.response!.IsSuccessStatusCode.Should().BeTrue(
			because: "the Parameters workspace is an operator-facing route");
		this.responseContent = await this.response.Content.ReadAsStringAsync();
	}

	[Then(@"the Parameters workspace selected Communications Address (.*) is visible")]
	public void ThenTheSelectedCommunicationsAddressIsVisible(string address)
	{
		this.responseContent.Should().Contain(address);
	}

	[Then(@"the Router Agent Type is visible")]
	public void ThenTheRouterAgentTypeIsVisible()
	{
		this.responseContent.Should().Contain("Router");
	}

	[Then(@"the Permanent, Non-Volatile, and Current Parameter Tables are available")]
	public void ThenTheParameterTablesAreAvailable()
	{
		this.responseContent.Should().Contain(">Permanent<");
		this.responseContent.Should().Contain(">Non-Volatile<");
		this.responseContent.Should().Contain(">Current<");
	}

	[Then(@"the workspace states that no Parameter values have been read")]
	public void ThenTheWorkspaceStatesThatNoParameterValuesHaveBeenRead()
	{
		this.responseContent.Should().Contain("No Parameter values have been read.");
	}

	[Then(@"Current is the selected Parameter Table")]
	public void ThenCurrentIsTheSelectedParameterTable()
	{
		this.responseContent.Should().Contain("<option selected>Current</option>");
	}

	[Then(@"Brigade or Agency is available to request through the local Router")]
	public void ThenBrigadeOrAgencyIsAvailableToRequestThroughTheLocalRouter()
	{
		this.responseContent.Should().Contain("Brigade or Agency");
		this.responseContent.Should().Contain(
			"action=\"/router/parameters/current/1\"");
	}

	[Then(@"the Parameter read status identifies Pending, Received, Rejected, Timed out, and Delivery failed")]
	public void ThenTheParameterReadStatusIdentifiesTerminalStates()
	{
		this.responseContent.Should().Contain("Pending");
		this.responseContent.Should().Contain("Received");
		this.responseContent.Should().Contain("Rejected");
		this.responseContent.Should().Contain("Timed out");
		this.responseContent.Should().Contain("Delivery failed");
	}

	[Then(@"the Current Router Parameter catalogue has Number, Name, Value, Read State, and Last Read columns")]
	public void ThenTheCurrentRouterParameterCatalogueHasRequiredColumns()
	{
		this.responseContent.Should().Contain("<caption>Current Router Parameter Table</caption>");
		this.responseContent.Should().Contain("<th scope=\"col\">Number</th>");
		this.responseContent.Should().Contain("<th scope=\"col\">Name</th>");
		this.responseContent.Should().Contain("<th scope=\"col\">Value</th>");
		this.responseContent.Should().Contain("<th scope=\"col\">Read State</th>");
		this.responseContent.Should().Contain("<th scope=\"col\">Last Read</th>");
	}

	[Then(@"the Brigade or Agency catalogue row has Parameter Number (.*)")]
	public void ThenTheBrigadeOrAgencyCatalogueRowHasParameterNumber(int parameterNumber)
	{
		this.responseContent.Should().Contain($"<td>{parameterNumber}</td>");
		this.responseContent.Should().Contain("<th scope=\"row\">Brigade or Agency</th>");
	}

	[Then(@"the Brigade or Agency catalogue row is Not read before a response")]
	public void ThenTheBrigadeOrAgencyCatalogueRowIsNotReadBeforeAResponse()
	{
		this.responseContent.Should().Contain(">Not read<");
	}

	[Then(@"the Current Routing Table entries (.*) through (.*) can be requested")]
	public void ThenTheCurrentRoutingTableEntriesCanBeRequested(
		int firstEntry,
		int lastEntry)
	{
		this.responseContent.Should().Contain("Current Routing Table entries");
		this.responseContent.Should().Contain(
			$"action=\"/router/parameters/current/13/entries/{firstEntry}-{lastEntry}\"");
		this.responseContent.Should().Contain($"value=\"{firstEntry}\"");
		this.responseContent.Should().Contain($"value=\"{lastEntry}\"");
	}

	[Then(@"the Routing Table request status identifies Pending, Received, Rejected, Timed out, and Delivery failed")]
	public void ThenTheRoutingTableRequestStatusIdentifiesTerminalStates()
	{
		this.responseContent.Should().Contain("Pending");
		this.responseContent.Should().Contain("Received");
		this.responseContent.Should().Contain("Rejected");
		this.responseContent.Should().Contain("Timed out");
		this.responseContent.Should().Contain("Delivery failed");
	}

	[Then(@"Next entries is unavailable until more values are returned")]
	public void ThenNextEntriesIsUnavailableUntilMoreValuesAreReturned()
	{
		this.responseContent.Should().Contain("id=\"routing-table-next-entries\"");
		this.responseContent.Should().Contain("disabled");
	}

	[Then(@"Permanent is the selected Parameter Table")]
	public void ThenPermanentIsTheSelectedParameterTable()
	{
		this.responseContent.Should().Contain("<option selected>Permanent</option>");
	}

	[Then(@"the Permanent Router Parameter Table is identified")]
	public void ThenThePermanentRouterParameterTableIsIdentified()
	{
		this.responseContent.Should().Contain("<caption>Permanent Router Parameter Table</caption>");
		this.responseContent.Should().Contain("Permanent Routing Table entries");
	}

	[Then(@"Brigade or Agency and Routing Table requests target the Permanent Parameter Table")]
	public void ThenParameterRequestsTargetThePermanentParameterTable()
	{
		this.responseContent.Should().Contain("action=\"/router/parameters/permanent/1\"");
		this.responseContent.Should().Contain(
			"action=\"/router/parameters/permanent/13/entries/1-1\"");
	}

	[Then(@"Network navigation retains Communications Address (.*)")]
	public void ThenNetworkNavigationRetainsCommunicationsAddress(string address)
	{
		this.responseContent.Should().Contain($"href=\"/network?address={address}\"");
	}

	[Then(@"Parameters is the active navigation workspace")]
	public void ThenParametersIsTheActiveNavigationWorkspace()
	{
		this.responseContent.Should().Contain(
			"<a class=\"navigation-link is-active\" href=\"/parameters");
	}

	[Then(@"the Current LAN MTA \(10\) Parameter Table is identified")]
	public void ThenTheCurrentLanMtaParameterTableIsIdentified()
	{
		this.responseContent.Should().Contain(
			"<caption>Current LAN MTA (10) Parameter Table</caption>");
	}

	[Then(@"the LAN MTA catalogue includes Port Number and Agent Type")]
	public void ThenTheLanMtaCatalogueIncludesPortNumberAndAgentType()
	{
		this.responseContent.Should().Contain("<th scope=\"row\">Port Number</th>");
		this.responseContent.Should().Contain("<th scope=\"row\">Agent Type</th>");
	}

	[Then(@"Port Number requests target participant port (.*)")]
	public void ThenPortNumberRequestsTargetParticipantPort(int port)
	{
		this.responseContent.Should().Contain(
			$"action=\"/participants/{port}/parameters/current/1\"");
	}

	[Then(@"the Router-only Routing Table browser is unavailable")]
	public void ThenTheRouterOnlyRoutingTableBrowserIsUnavailable()
	{
		this.responseContent.Should().NotContain("Routing Table entries");
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
