using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Reqnroll;
using Router.Persistence;
using Stensones.GD92.Fields;
using System.Net;
using System.Globalization;
using System.Text.Json;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class RouterParameterRequestSteps
{
	private static readonly RouterParameterRequestApplicationPool applications = new();

	private DistributedApplication? application;
	private HttpClient? client;
	private HttpResponseMessage? response;
	private string? level1Password;
	private string? pageContent;
	private Uri? inventoryScanStatusAddress;
	private IReadOnlyDictionary<byte, Uri>? routerParameterStatusAddresses;
	private IReadOnlyDictionary<byte, Uri>? participantParameterStatusAddresses;
	private ushort? lastReturnedRoutingTableEntry;
	private RouterParameterRequestApplicationProfile applicationProfile;

	[Given(@"NodeManager is the User Agent at Brigade (.*), Node (.*), and Port (.*)")]
	public void GivenNodeManagerIsTheUserAgentAt(byte brigade, ushort node, byte port)
	{
		(brigade, node, port).Should().Be((26, 100, 25));
	}

	[Given(@"its local Router is at Brigade (.*), Node (.*), and Port (.*)")]
	public void GivenItsLocalRouterIsAt(byte brigade, ushort node, byte port)
	{
		(brigade, node, port).Should().Be((26, 100, 0));
	}

	[Given(@"NodeManager's configured local Router does not respond")]
	public void GivenNodeManagersConfiguredLocalRouterDoesNotRespond()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.NonrespondingRouter;
	}

	[BeforeScenario("HighConcurrencyInventoryScan")]
	public void UseHighConcurrencyInventoryScan()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.HighConcurrencyInventoryScan;
	}

	[Given(@"the local Router has Routing Table entry 1 to next node 26.101.0")]
	public async Task GivenTheLocalRouterHasRoutingTableEntryOne()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.RouterWithRoutingTableEntry;
		await this.EnsureApplicationStartedAsync();
		await this.ConfigureLocalRouterRoutingTableAsync(101);
	}

	[Given(@"the local Router has Routing Table entries 1 and 2")]
	public async Task GivenTheLocalRouterHasRoutingTableEntriesOneAndTwo()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.RouterWithRoutingTableEntry;
		await this.EnsureApplicationStartedAsync();
		await this.ConfigureLocalRouterRoutingTableAsync(101, 102);
	}

	[Given(@"the local Router has Routing Table entries 1 through 200")]
	public async Task GivenTheLocalRouterHasRoutingTableEntriesOneThroughTwoHundred()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.RouterWithRoutingTableEntry;
		await this.EnsureApplicationStartedAsync();
		await this.ConfigureLocalRouterRoutingTableAsync(
			[.. Enumerable.Range(101, 200).Select(number => (ushort)number)]);
	}

	private async Task ConfigureLocalRouterRoutingTableAsync(params ushort[] nextNodeNumbers)
	{
		var connectionString = await this.application!.GetConnectionStringAsync("router-database")
			?? throw new InvalidOperationException(
				"The test Router database connection string was not provided.");
		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(connectionString)
			.Options;
		await using var database = new RouterDbContext(options);
		var parameterStore = new EfRouterParameterStore(database);
		var routingTable = RoutingTable.FromEntries(
			[.. nextNodeNumbers.Select((nextNodeNumber, offset) =>
				RoutingTableEntry.FromValues(
					ParameterEntryIndex.FromValue((ushort)(offset + 1)),
					ProtocolBoolean.True,
					CommunicationsAddress.FromValues(
						Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
						Node.FromValue(NodeIdentifier.FromValue(nextNodeNumber)),
						Port.FromValue(PortIdentifier.FromValue(0))),
					DestinationNodes.FromAddressRanges(),
					AgentType.FromValue(AgentTypeValue.LanMessageTransferAgent),
					RoutingPreference.FromValue(0)))]);

		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.RouterTable.Number,
			RouterParameterCatalogue.RouterTable.Encode(routingTable));
	}

	[Given(@"the local Router has PSTN Table entry 1 to next node 26.101.0, telephone number 12, hold time 30, used, and available")]
	public async Task GivenTheLocalRouterHasPstnTableEntryOne()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.RouterWithRoutingTableEntry;
		await this.EnsureApplicationStartedAsync();

		var connectionString = await this.application!.GetConnectionStringAsync("router-database")
			?? throw new InvalidOperationException(
				"The test Router database connection string was not provided.");
		var options = new DbContextOptionsBuilder<RouterDbContext>()
			.UseNpgsql(connectionString)
			.Options;
		await using var database = new RouterDbContext(options);
		var parameterStore = new EfRouterParameterStore(database);
		var entry = TelephoneTableEntry.FromValues(
			ParameterEntryIndex.FromValue(1),
			ProtocolBoolean.True,
			CommunicationsAddress.FromValues(
				Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
				Node.FromValue(NodeIdentifier.FromValue(101)),
				Port.FromValue(PortIdentifier.FromValue(0))),
			TelephoneNumber.FromValue(SevenBitAsciiString.FromValue("12")),
			HoldTime.FromValue(30),
			ProtocolBoolean.True);
		await parameterStore.StoreAsync(
			ParameterTable.NonVolatile,
			RouterParameterCatalogue.PstnTable.Number,
			RouterParameterCatalogue.PstnTable.Encode(PstnTable.FromEntries(entry)));
	}

	[When(@"I request the local Router brigade or agency number")]
	public async Task WhenIRequestTheLocalRouterBrigadeOrAgencyNumber()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/brigade-or-agency-number", null);
	}

	[When(@"I request local Router Current Parameter 99")]
	public async Task WhenIRequestLocalRouterCurrentParameterNinetyNine()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/99", null);
	}

	[When(@"I request local Router Current Parameter 2")]
	public async Task WhenIRequestLocalRouterCurrentParameterTwo()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/2", null);
	}

	[When(@"I request local Router Current Parameter 3")]
	public async Task WhenIRequestLocalRouterCurrentParameterThree()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/3", null);
	}

	[When(@"I request local Router Current Parameter 9")]
	public async Task WhenIRequestLocalRouterCurrentParameterNine()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/9", null);
	}

	[When(@"I request local Router Current Parameter 10")]
	public async Task WhenIRequestLocalRouterCurrentParameterTen()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/10", null);
	}

	[When(@"I request local Router Current Parameter 11")]
	public async Task WhenIRequestLocalRouterCurrentParameterEleven()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/11", null);
	}

	[When(@"I request local Router Current Parameter 18")]
	public async Task WhenIRequestLocalRouterCurrentParameterEighteen()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/18", null);
	}

	[When(@"I request local Router Current Parameter 20")]
	public async Task WhenIRequestLocalRouterCurrentParameterTwenty()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/current/20", null);
	}

	[When(@"I request local Router Current PSTN Table entries 1 through 1")]
	public async Task WhenIRequestLocalRouterCurrentPstnTableEntries()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/current/14/entries/1-1",
			null);
	}

	[When(@"I request local Router Routing Table entry 1")]
	public async Task WhenIRequestLocalRouterRoutingTableEntryOne()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/current/13/entries/1-1",
			null);
	}

	[When(@"I request local Router Routing Table entry 2")]
	public async Task WhenIRequestLocalRouterRoutingTableEntryTwo()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/current/13/entries/2-2",
			null);
	}

	[When(@"I request local Router Routing Table entries 1 through 2")]
	public async Task WhenIRequestLocalRouterRoutingTableEntriesOneThroughTwo()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/current/13/entries/1-2",
			null);
	}

	[When(@"I request the next local Router Routing Table entry")]
	public async Task WhenIRequestTheNextLocalRouterRoutingTableEntry()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/current/13/entries/2-2",
			null);
	}

	[When(@"I request local Router Routing Table entries 1 through 200")]
	public async Task WhenIRequestLocalRouterRoutingTableEntriesOneThroughTwoHundred()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/current/13/entries/1-200",
			null);
	}

	[When(@"I request the remaining local Router Routing Table entries")]
	public async Task WhenIRequestTheRemainingLocalRouterRoutingTableEntries()
	{
		await this.EnsureApplicationStartedAsync();
		var nextFirstEntry = (ushort)(this.lastReturnedRoutingTableEntry!.Value + 1);

		this.response = await this.client!.PostAsync(
			$"/router/parameters/current/13/entries/{nextFirstEntry}-200",
			null);
	}

	[When(@"I request LAN MTA Current Parameter (.*)")]
	public async Task WhenIRequestLanMtaCurrentParameter(byte parameterNumber)
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			$"/participants/1/parameters/current/{parameterNumber}",
			null);
	}

	[When(@"I request LAN MTA Non-Volatile Parameter (.*)")]
	public async Task WhenIRequestLanMtaNonVolatileParameter(byte parameterNumber)
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			$"/participants/1/parameters/non-volatile/{parameterNumber}",
			null);
	}

	[When(@"I request LAN MTA Permanent Parameter (.*)")]
	public async Task WhenIRequestLanMtaPermanentParameter(byte parameterNumber)
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			$"/participants/1/parameters/permanent/{parameterNumber}",
			null);
	}

	[When(@"I request every LAN MTA Current Parameter")]
	public async Task WhenIRequestEveryLanMtaCurrentParameter()
	{
		await this.EnsureApplicationStartedAsync();

		var requests = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 21 }
			.Select(async parameterNumber =>
			{
				using var response = await this.client!.PostAsync(
					$"/participants/1/parameters/current/{parameterNumber}",
					null);
				response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
				response.Headers.Location.Should().NotBeNull();
				return (parameterNumber, statusAddress: response.Headers.Location!);
			});
		var responses = await Task.WhenAll(requests);
		this.participantParameterStatusAddresses = responses.ToDictionary(
			response => response.parameterNumber,
			response => response.statusAddress);
	}

	[When(@"I request every Printer UA Current Parameter")]
	public async Task WhenIRequestEveryPrinterUaCurrentParameter()
	{
		await this.EnsureApplicationStartedAsync();

		var requests = new byte[] { 1, 2, 3, 21, 22, 23, 24 }
			.Select(async parameterNumber =>
			{
				using var response = await this.client!.PostAsync(
					$"/participants/2/parameters/current/{parameterNumber}",
					null);
				response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
				response.Headers.Location.Should().NotBeNull();
				return (parameterNumber, statusAddress: response.Headers.Location!);
			});
		var responses = await Task.WhenAll(requests);
		this.participantParameterStatusAddresses = responses.ToDictionary(
			response => response.parameterNumber,
			response => response.statusAddress);
	}

	[When(@"I request every Network Management UA Current Parameter")]
	public async Task WhenIRequestEveryNetworkManagementUaCurrentParameter()
	{
		await this.EnsureApplicationStartedAsync();

		var requests = new byte[] { 1, 2, 3 }
			.Select(async parameterNumber =>
			{
				using var response = await this.client!.PostAsync(
					$"/participants/25/parameters/current/{parameterNumber}",
					null);
				response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
				response.Headers.Location.Should().NotBeNull();
				return (parameterNumber, statusAddress: response.Headers.Location!);
			});
		var responses = await Task.WhenAll(requests);
		this.participantParameterStatusAddresses = responses.ToDictionary(
			response => response.parameterNumber,
			response => response.statusAddress);
	}

	[When(@"I open NodeManager")]
	public async Task WhenIOpenNodeManager()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.GetAsync("/");
		this.response.EnsureSuccessStatusCode();
		this.pageContent = await this.response.Content.ReadAsStringAsync();
	}

	[Then(@"NodeManager presents a Node Login form that securely submits password, brigade, node, and port")]
	public void ThenNodeManagerPresentsANodeLoginForm()
	{
		this.pageContent.Should().Contain("id=\"router-logon\"");
		this.pageContent.Should().Contain("action=\"/router/parameters/logon\"");
		this.pageContent.Should().Contain("method=\"post\"");
		this.pageContent.Should().Contain("""type="password" name="password" required""");
		this.pageContent.Should().Contain("""type="number" name="brigade""");
		this.pageContent.Should().Contain("""type="number" name="node""");
		this.pageContent.Should().Contain("""type="number" name="port""");
		this.pageContent.Should().Contain("""type="submit">Log on</button>""");
	}

	[Then(@"NodeManager presents Routing Table entry selection")]
	public void ThenNodeManagerPresentsRoutingTableEntrySelection()
	{
		this.pageContent.Should().Contain("id=\"router-routing-table-entry-selection\"");
	}

	[Then(@"NodeManager presents a hidden Routing Table next-page control")]
	public void ThenNodeManagerPresentsAHiddenRoutingTableNextPageControl()
	{
		this.pageContent.Should().Contain("""id="router-routing-table-next-page" hidden""");
	}

	[Then(@"NodeManager requests the next contiguous Routing Table entry range")]
	public void ThenNodeManagerRequestsTheNextContiguousRoutingTableEntryRange()
	{
		this.pageContent.Should().Contain(
			"const routerRoutingTableNextPage = document.getElementById(" +
			"\"router-routing-table-next-page\");");
		this.pageContent.Should().Contain(
			"routerRoutingTableNextPage.addEventListener(\"click\", () =>");
		this.pageContent.Should().Contain(
			"requestRoutingTableEntries(nextRoutingTableEntryRange.firstEntry, " +
			"nextRoutingTableEntryRange.lastEntry);");
		this.pageContent.Should().Contain("const hasMoreValues = request.moreValues === true;");
		this.pageContent.Should().Contain(
			"const hasNextPage = nextRoutingTableEntryRange !== null;");
		this.pageContent.Should().Contain("firstEntry: entries.at(-1).index + 1,");
		this.pageContent.Should().Contain("lastEntry }");
		this.pageContent.Should().Contain(
			"routerRoutingTableNextPage.hidden = !hasNextPage;");
	}

	[Then(@"NodeManager enables Routing Table entry selection")]
	public void ThenNodeManagerEnablesRoutingTableEntrySelection()
	{
		this.pageContent.Should().Contain("id=\"router-routing-table-entry-selection\"");
		this.pageContent.Should().NotContain(
			"id=\"router-routing-table-entry-selection\" disabled");
	}

	[Then(@"NodeManager submits selected Routing Table entries through GD-92")]
	public void ThenNodeManagerSubmitsSelectedRoutingTableEntries()
	{
		this.pageContent.Should().Contain(
			"const routerRoutingTableEntrySelection = document.getElementById(" +
			"\"router-routing-table-entry-selection\");");
		this.pageContent.Should().Contain(
			"/router/parameters/current/13/entries/${firstEntry}-${lastEntry}");
	}

	[Then(@"NodeManager renders returned Routing Table entries")]
	public void ThenNodeManagerRendersReturnedRoutingTableEntries()
	{
		this.pageContent.Should().Contain(
			"Routing Table entry ${entry.index}: next node ${entry.nextNode}");
	}

	[Then(@"NodeManager follows a Node Login status redirect")]
	public void ThenNodeManagerFollowsANodeLoginStatusRedirect()
	{
		this.pageContent.Should().NotContain("redirect: \"manual\"");
		this.pageContent.Should().Contain("const statusUrl = response.url;");
	}

	[Then(@"NodeManager presents an enabled Discover local participants control")]
	public void ThenNodeManagerPresentsAnEnabledDiscoverLocalParticipantsControl()
	{
		this.pageContent.Should().Contain("id=\"router-participant-discovery\"");
		this.pageContent.Should().Contain("action=\"/router/participants/discovery\"");
		this.pageContent.Should().Contain("method=\"post\"");
		this.pageContent.Should().Contain("""type="submit">Discover local participants</button>""");
		this.pageContent.Should().NotContain("""type="submit" disabled>Discover local participants</button>""");
	}

	[Then(@"NodeManager presents Inventory Scan progress and result areas")]
	public void ThenNodeManagerPresentsInventoryScanProgressAndResultAreas()
	{
		this.pageContent.Should().Contain(
			"""<p id="router-participant-discovery-status" role="status" hidden></p>""");
		this.pageContent.Should().Contain(
			"""<table id="router-participant-discovery-results" hidden>""");
		this.pageContent.Should().Contain(
			"""<div id="router-participant-discovery-summary" hidden></div>""");
		this.pageContent.Should().Contain(
			"""const discoveryForm = document.getElementById("router-participant-discovery");""");
		this.pageContent.Should().Contain(
			"""discoveryButton.disabled = true;""");
		this.pageContent.Should().Contain(
			"""Inventory Scan: ${scan.completedProbeCount} of 63 probes completed.""");
	}

	[Then(@"NodeManager presents Parameter selection and result areas for every discovered participant")]
	public void ThenNodeManagerPresentsParameterSelectionAndResultAreasForEveryDiscoveredParticipant()
	{
		this.pageContent.Should().Contain("""<th scope="col">Parameters</th>""");
		this.pageContent.Should().Contain("""id="router-parameter-list" hidden""");
		this.pageContent.Should().Contain("""viewParametersButton.disabled = scan.completedProbeCount < 63;""");
	}

	[When(@"I select Discover local participants")]
	public async Task WhenISelectDiscoverLocalParticipants()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/participants/discovery", null);
	}

	[When(@"I select View parameters for the discovered Router")]
	public async Task WhenISelectViewParametersForTheDiscoveredRouter()
	{
		await this.EnsureApplicationStartedAsync();

		using var browserClient = CreateRedirectFollowingClient(this.application!);
		var requests = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 19 }
			.Select(async parameterNumber =>
			{
				using var response = await browserClient.PostAsync(
					$"/router/parameters/current/{parameterNumber}",
					null);
				response.StatusCode.Should().Be(HttpStatusCode.OK);
				response.RequestMessage!.RequestUri.Should().NotBeNull();
				return (parameterNumber, statusAddress: response.RequestMessage.RequestUri!);
			});
		var responses = await Task.WhenAll(requests);
		this.routerParameterStatusAddresses = responses.ToDictionary(
			response => response.parameterNumber,
			response => response.statusAddress);
	}

	[Then(@"I am redirected to a pending Inventory Scan status")]
	public void ThenIAmRedirectedToAPendingInventoryScanStatus()
	{
		this.response!.StatusCode.Should().Be(HttpStatusCode.SeeOther);
		this.response.Headers.Location.Should().NotBeNull();
		this.inventoryScanStatusAddress = this.response.Headers.Location;
	}

	[Then(@"the Inventory Scan status reports progress before completion")]
	public async Task ThenTheInventoryScanStatusReportsProgressBeforeCompletion()
	{
		await this.WaitForInventoryScanStatusAsync(status =>
			status.GetProperty("completedProbeCount").GetInt32() > 0 &&
			status.GetProperty("completedProbeCount").GetInt32() < 63);
	}

	[Then(@"the completed Inventory Scan lists Router port 0, LAN MTA port 1, Printer User Agent port 2, and Network Management User Agent port 25")]
	public async Task ThenTheCompletedInventoryScanListsLocalParticipants()
	{
		using var status = await this.WaitForInventoryScanStatusAsync(
			document => document.GetProperty("completedProbeCount").GetInt32() == 63);
		var participants = status.RootElement.GetProperty("participants").EnumerateArray().ToArray();

		participants.Should().Contain(participant =>
			participant.GetProperty("port").GetByte() == 0 &&
			participant.GetProperty("kind").GetString() == "router");
		participants.Should().Contain(participant =>
			participant.GetProperty("port").GetByte() == 1 &&
			participant.GetProperty("kind").GetString() == "mta" &&
			participant.GetProperty("agentType").GetString() == "LAN MTA (10)");
		participants.Should().Contain(participant =>
			participant.GetProperty("port").GetByte() == 2 &&
			participant.GetProperty("kind").GetString() == "ua" &&
			participant.GetProperty("agentType").GetString() == "Printer (4)");
		participants.Should().Contain(participant =>
			participant.GetProperty("port").GetByte() == 25 &&
			participant.GetProperty("kind").GetString() == "ua" &&
			participant.GetProperty("agentType").GetString() == "Network Management UA (12)");
	}

	[Then(@"NodeManager lists the local Router Current Parameters with their received values")]
	public async Task ThenNodeManagerListsTheLocalRouterCurrentParametersWithTheirReceivedValues()
	{
		var expectedValues = new Dictionary<byte, string>
		{
			[1] = "26",
			[2] = "100",
			[3] = "Station End",
			[4] = "Level 0, User-Agent 26.100.0, PASSWORD",
			[5] = "PASSWORD",
			[6] = "PASSWORD",
			[7] = "PASSWORD",
			[8] = "PASSWORD",
			[9] = "1023",
			[10] = "26.100.25",
			[11] = "26.100.25",
			[12] = "5",
			[19] = "3"
		};

		foreach (var (parameterNumber, expectedValue) in expectedValues)
		{
			using var status = await this.WaitForRouterParameterStatusAsync(
				this.routerParameterStatusAddresses![parameterNumber]);

			status.RootElement.GetProperty("state").GetString().Should().Be("received");
			status.RootElement.GetProperty("parameterNumber").GetByte().Should().Be(parameterNumber);
			status.RootElement.GetProperty("parameterValue").GetString().Should().Be(expectedValue);
		}
	}

	[Then(@"NodeManager redacts the local Router Password Parameters")]
	public void ThenNodeManagerRedactsTheLocalRouterPasswordParameters()
	{
		this.pageContent.Should().Contain("PASSWORD");
		this.pageContent.Should().NotContain("FIRE1");
	}

	[Then(@"NodeManager presents Current Parameter catalogues for LAN MTA, Printer UA, and Network Management UA")]
	public void ThenNodeManagerPresentsCurrentParameterCataloguesForAllDiscoveredParticipants()
	{
		this.pageContent.Should().Contain("const participantCurrentParameterCatalogues = {");
		this.pageContent.Should().Contain("LAN MTA (10)");
		this.pageContent.Should().Contain("Printer (4)");
		this.pageContent.Should().Contain("Network Management UA (12)");
		this.pageContent.Should().Contain(
			"/participants/${participant.port}/parameters/${parameterTable}/${parameter.number}");
	}

	[Then(@"NodeManager presents a Parameter Table selector")]
	public void ThenNodeManagerPresentsAParameterTableSelector()
	{
		this.pageContent.Should().Contain("id=\"participant-parameter-table\"");
		this.pageContent.Should().Contain("""value="permanent">Permanent</option>""");
		this.pageContent.Should().Contain("""value="non-volatile">Non-Volatile</option>""");
		this.pageContent.Should().Contain("""value="current" selected>Current</option>""");
		this.pageContent.Should().Contain(
			"/participants/${participant.port}/parameters/${parameterTable}/${parameter.number}");
	}

	[Then(@"the completed Inventory Scan summary shows (.*) discovered participants, (.*) timeouts, no delivery failures, and no negative acknowledgements")]
	public async Task ThenTheCompletedInventoryScanSummaryShows(
		int discoveredParticipants,
		int timeouts)
	{
		using var status = await this.WaitForInventoryScanStatusAsync(
			document => document.GetProperty("completedProbeCount").GetInt32() == 63);
		var summary = status.RootElement.GetProperty("summary");

		summary.GetProperty("discoveredParticipantCount").GetInt32().Should().Be(discoveredParticipants);
		summary.GetProperty("timeoutCount").GetInt32().Should().Be(timeouts);
		summary.GetProperty("deliveryFailureCount").GetInt32().Should().Be(0);
		summary.GetProperty("negativeAcknowledgements").EnumerateObject().Should().BeEmpty();
	}

	[When(@"I log on User-Agent address Brigade (.*), Node (.*), and Port (.*) with the Level 1 password")]
	public async Task WhenILogOnUserAgentAddressWithTheLevel1Password(
		byte brigade,
		ushort node,
		byte port)
	{
		await this.LogOnUserAgentAddressAsync(brigade, node, port, this.level1Password!);
	}

	[When(@"I log on User-Agent address Brigade (.*), Node (.*), and Port (.*) with the incorrect password ""(.*)""")]
	public async Task WhenILogOnUserAgentAddressWithTheIncorrectPassword(
		byte brigade,
		ushort node,
		byte port,
		string password)
	{
		await this.LogOnUserAgentAddressAsync(brigade, node, port, password);
	}

	private async Task LogOnUserAgentAddressAsync(
		byte brigade,
		ushort node,
		byte port,
		string password)
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync(
			"/router/parameters/logon",
			new FormUrlEncodedContent(
			[
				new KeyValuePair<string, string>("password", password),
				new KeyValuePair<string, string>("brigade", brigade.ToString()),
				new KeyValuePair<string, string>("node", node.ToString()),
				new KeyValuePair<string, string>("port", port.ToString())
			]));
	}

	[When(@"I log off the local Router")]
	public async Task WhenILogOffTheLocalRouter()
	{
		this.response = await this.client!.PostAsync("/router/parameters/logoff", null);
	}

	[Given(@"the Router persistent Parameter Tables are empty")]
	public async Task GivenTheRouterPersistentParameterTablesAreEmpty()
	{
		this.applicationProfile = RouterParameterRequestApplicationProfile.FreshParameterTables;
		await this.EnsureApplicationStartedAsync();
	}

	[Given(@"the Router Level 1 password is ""(.*)""")]
	public void GivenTheRouterLevel1PasswordIs(string password)
	{
		this.level1Password = password;
	}

	[Then(@"I am redirected to the pending Parameter Request status")]
	public async Task ThenIAmRedirectedToThePendingParameterRequestStatus()
	{
		if (this.response!.StatusCode != HttpStatusCode.SeeOther)
		{
			var error = await this.response.Content.ReadAsStringAsync();

			throw new Xunit.Sdk.XunitException(
				$"Expected HTTP {HttpStatusCode.SeeOther}, received {this.response.StatusCode}: {error}");
		}

		this.response.Headers.Location.Should().NotBeNull();
	}

	[Then(@"I am redirected to the pending Participant Parameter Request status")]
	public Task ThenIAmRedirectedToThePendingParticipantParameterRequestStatus()
	{
		return this.ThenIAmRedirectedToThePendingParameterRequestStatus();
	}

	[Then(@"I am redirected to the pending Node Login status")]
	public async Task ThenIAmRedirectedToThePendingNodeLoginStatus()
	{
		await this.ThenIAmRedirectedToThePendingParameterRequestStatus();
	}

	[Then(@"the Parameter Request status eventually shows brigade or agency number (.*)")]
	public async Task ThenTheParameterRequestStatusEventuallyShowsBrigadeOrAgencyNumber(byte brigadeOrAgencyNumber)
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = await this.client!.GetAsync(statusAddress);
			var content = await status.Content.ReadAsStringAsync();

			if (status.StatusCode == HttpStatusCode.OK &&
				content.Contains(brigadeOrAgencyNumber.ToString(), StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			$"The Parameter Request status did not show brigade or agency number {brigadeOrAgencyNumber}.");
	}

	[Then(@"the Parameter Request status eventually shows Routing Table entry 1 to next node 26.101.0")]
	public async Task ThenTheParameterRequestStatusEventuallyShowsRoutingTableEntryOne()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("routingTableEntries")[0]
						.GetProperty("index").GetInt32() == 1 &&
					document.RootElement.GetProperty("routingTableEntries")[0]
						.GetProperty("nextNode").GetString() == "26.101.0")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Routing Table entry 1 to next node 26.101.0.");
	}

	[Then(@"the Parameter Request status shows Routing Table entry 1 to next node 26.101.0 with more values")]
	public Task ThenTheParameterRequestStatusShowsRoutingTableEntryOneWithMoreValues()
	{
		return this.ThenTheParameterRequestStatusShowsRoutingTableEntryWithMoreValues(
			1,
			"26.101.0",
			hasMoreValues: true);
	}

	[Then(@"the Parameter Request status shows Routing Table entry 2 to next node 26.102.0 with no more values")]
	public Task ThenTheParameterRequestStatusShowsRoutingTableEntryTwoWithNoMoreValues()
	{
		return this.ThenTheParameterRequestStatusShowsRoutingTableEntryWithMoreValues(
			2,
			"26.102.0",
			hasMoreValues: false);
	}

	private async Task ThenTheParameterRequestStatusShowsRoutingTableEntryWithMoreValues(
		ushort expectedIndex,
		string expectedNextNode,
		bool hasMoreValues)
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("routingTableEntries")[0]
						.GetProperty("index").GetInt32() == expectedIndex &&
					document.RootElement.GetProperty("routingTableEntries")[0]
						.GetProperty("nextNode").GetString() == expectedNextNode &&
					document.RootElement.GetProperty("moreValues").GetBoolean() == hasMoreValues)
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			$"The Parameter Request status did not show Routing Table entry {expectedIndex} " +
			$"to next node {expectedNextNode} with more values {hasMoreValues}.");
	}

	[Then(@"the Parameter Request status shows a capacity-limited Routing Table page with more values")]
	public async Task ThenTheParameterRequestStatusShowsACapacityLimitedRoutingTablePage()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				var entries = document.RootElement.GetProperty("routingTableEntries");
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("moreValues").GetBoolean() &&
					entries.GetArrayLength() is > 0 and < 200 &&
					entries[0].GetProperty("index").GetInt32() == 1)
				{
					this.lastReturnedRoutingTableEntry = (ushort)entries[entries.GetArrayLength() - 1]
						.GetProperty("index").GetInt32();
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show a capacity-limited Routing Table page.");
	}

	[Then(@"the Parameter Request status shows the final Routing Table page through entry 200")]
	public async Task ThenTheParameterRequestStatusShowsTheFinalRoutingTablePage()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				var entries = document.RootElement.GetProperty("routingTableEntries");
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					!document.RootElement.GetProperty("moreValues").GetBoolean() &&
					entries[0].GetProperty("index").GetInt32() ==
					this.lastReturnedRoutingTableEntry!.Value + 1 &&
					entries[entries.GetArrayLength() - 1].GetProperty("index").GetInt32() == 200)
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show the final Routing Table page through entry 200.");
	}

	[Then(@"the Parameter Request status shows Parameter \/ Invalid Entry rejection")]
	public Task ThenTheParameterRequestStatusShowsParameterInvalidEntryRejection()
	{
		return this.ThenTheParameterRequestStatusShowsParameterRejection(
			"Parameter / Invalid Entry");
	}

	[Then(@"the Parameter Request status shows Parameter \/ Invalid Parameter rejection")]
	public Task ThenTheParameterRequestStatusShowsParameterInvalidParameterRejection()
	{
		return this.ThenTheParameterRequestStatusShowsParameterRejection(
			"Parameter / Invalid Parameter");
	}

	private async Task ThenTheParameterRequestStatusShowsParameterRejection(
		string expectedRejectionReason)
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "rejected" &&
					document.RootElement.GetProperty("rejectionReason").GetString() ==
					expectedRejectionReason)
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			$"The Parameter Request status did not show a {expectedRejectionReason} rejection.");
	}

	[Then(@"the Parameter Request status shows Router Current Node Number 100")]
	public async Task ThenTheParameterRequestStatusShowsRouterCurrentNodeNumber()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 2 &&
					document.RootElement.GetProperty("parameterValue").GetString() == "100")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Router Current Node Number 100.");
	}

	[Then(@"NodeManager lists Node Number in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsNodeNumberInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 2, name: "Node Number" },""");
	}

	[Then(@"the Parameter Request status shows Router Current Node Name Station End")]
	public async Task ThenTheParameterRequestStatusShowsRouterCurrentNodeName()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 3 &&
					document.RootElement.GetProperty("parameterValue").GetString() == "Station End")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Router Current Node Name Station End.");
	}

	[Then(@"NodeManager lists Node Name in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsNodeNameInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 3, name: "Node Name" },""");
	}

	[Then(@"the Parameter Request status shows Router Current Maximum Message Length 1023")]
	public async Task ThenTheParameterRequestStatusShowsRouterCurrentMaximumMessageLength()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 9 &&
					document.RootElement.GetProperty("parameterValue").GetString() == "1023")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Router Current Maximum Message Length 1023.");
	}

	[Then(@"NodeManager lists Maximum Message Length in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsMaximumMessageLengthInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 9, name: "Maximum Message Length" },""");
	}

	[Then(@"the Parameter Request status shows Router Current Network Manager Address 1 26.100.25")]
	public async Task ThenTheParameterRequestStatusShowsRouterCurrentNetworkManagerAddressOne()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 10 &&
					document.RootElement.GetProperty("parameterValue").GetString() == "26.100.25")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Router Current Network Manager Address 1 26.100.25.");
	}

	[Then(@"NodeManager lists Network Manager Address 1 in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsNetworkManagerAddressOneInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 10, name: "Network Manager Address 1" },""");
	}

	[Then(@"the Parameter Request status shows Router Current Network Manager Address 2 26.100.25")]
	public async Task ThenTheParameterRequestStatusShowsRouterCurrentNetworkManagerAddressTwo()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 11 &&
					document.RootElement.GetProperty("parameterValue").GetString() == "26.100.25")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Router Current Network Manager Address 2 26.100.25.");
	}

	[Then(@"NodeManager lists Network Manager Address 2 in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsNetworkManagerAddressTwoInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 11, name: "Network Manager Address 2" },""");
	}

	[Then(@"the Parameter Request status shows Router Current Manual Acknowledgement Timeout 60")]
	public async Task ThenTheParameterRequestStatusShowsRouterCurrentManualAcknowledgementTimeout()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 18 &&
					document.RootElement.GetProperty("parameterValue").GetString() == "60")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show Router Current Manual Acknowledgement Timeout 60.");
	}

	[Then(@"NodeManager lists Manual Acknowledgement Timeout in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsManualAcknowledgementTimeoutInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 18, name: "Manual Acknowledgement Timeout" },""");
	}

	[Then(@"the Parameter Request status shows the current Router UTC Time and Date")]
	public async Task ThenTheParameterRequestStatusShowsTheCurrentRouterUtcTimeAndDate()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterNumber").GetInt32() == 20 &&
					DateTime.TryParseExact(
						document.RootElement.GetProperty("parameterValue").GetString(),
						"ddMMMyyHHmmss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
						out var returnedUtcTime) &&
					Math.Abs((DateTime.UtcNow - returnedUtcTime).TotalMinutes) <= 1)
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show the current Router UTC Time and Date.");
	}

	[Then(@"NodeManager lists Time and Date in the Router Current Parameter catalogue")]
	public void ThenNodeManagerListsTimeAndDateInTheRouterCurrentParameterCatalogue()
	{
		this.pageContent.Should().Contain("""{ number: 20, name: "Time and Date" },""");
	}

	[Then(@"NodeManager renders the Routing Table rejection reason")]
	public void ThenNodeManagerRendersTheRoutingTableRejectionReason()
	{
		this.pageContent.Should().Contain(
			"Routing Table request ${request.state}: ${request.rejectionReason}.");
	}

	[Then(@"the Parameter Request status shows PSTN Table entry 1 as used and available with next node 26.101.0, telephone number 12, and hold time 30")]
	public async Task ThenTheParameterRequestStatusShowsPstnTableEntryOne()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.TryGetProperty("pstnTableEntries", out var entries) &&
					entries.GetArrayLength() == 1)
				{
					var entry = entries[0];
					if (entry.GetProperty("index").GetInt32() == 1 &&
						entry.GetProperty("used").GetBoolean() &&
						entry.GetProperty("nextNode").GetString() == "26.101.0" &&
						entry.GetProperty("telephoneNumber").GetString() == "12" &&
						entry.GetProperty("holdTime").GetInt32() == 30 &&
						entry.GetProperty("available").GetBoolean())
					{
						return;
					}
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show the expected PSTN Table entry.");
	}

	[Then(@"the Parameter Request status eventually shows timed-out")]
	public async Task ThenTheParameterRequestStatusEventuallyShowsTimedOut()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 10; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				var content = await status.Content.ReadAsStringAsync();
				using var document = JsonDocument.Parse(content);
				if (document.RootElement.GetProperty("state").GetString() == "timed-out")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Parameter Request status did not show timed-out.");
	}

	[Then(@"the Participant Parameter Request status eventually shows interface status Idle")]
	public async Task ThenTheParticipantParameterRequestStatusEventuallyShowsInterfaceStatusIdle()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				var content = await status.Content.ReadAsStringAsync();
				using var document = JsonDocument.Parse(content);
				if (document.RootElement.GetProperty("state").GetString() == "received" &&
					document.RootElement.GetProperty("parameterValue").GetString() == "Idle")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Participant Parameter Request did not return LAN MTA interface status Idle.");
	}

	[Then(@"the Participant Parameter Request status eventually shows retained value (.*)")]
	public async Task ThenTheParticipantParameterRequestStatusEventuallyShowsRetainedValue(
		byte expectedValue)
	{
		var statusAddress = this.response!.Headers.Location!;
		string? lastStatus = null;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			var content = await status.Content.ReadAsStringAsync();
			lastStatus = $"{status.StatusCode}: {content}";
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var result = JsonDocument.Parse(content);
				if (result.RootElement.GetProperty("state").GetString() == "received" &&
					result.RootElement.GetProperty("parameterValue").GetString() ==
					expectedValue.ToString())
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			$"The Participant Parameter Request did not return retained value {expectedValue}. " +
			$"Last status: {lastStatus}{await this.GetResourceLogAsync("LAN-MTA")}");
	}

	[Then(@"the Participant Parameter Request status eventually shows rejected")]
	public async Task ThenTheParticipantParameterRequestStatusEventuallyShowsRejected()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			using var status = await this.client!.GetAsync(statusAddress);
			if (status.StatusCode == HttpStatusCode.OK)
			{
				using var document = JsonDocument.Parse(await status.Content.ReadAsStringAsync());
				if (document.RootElement.GetProperty("state").GetString() == "rejected")
				{
					return;
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Participant Parameter Request did not return a negative acknowledgement.");
	}

	[Then(@"the Participant Parameter Request statuses show the LAN MTA Current values")]
	public async Task ThenTheParticipantParameterRequestStatusesShowTheLanMtaCurrentValues()
	{
		var expectedValues = new Dictionary<byte, string>
		{
			[1] = "1",
			[2] = "LAN MTA (10)",
			[3] = "Idle",
			[4] = "true",
			[5] = "0",
			[6] = "0",
			[7] = "0",
			[8] = "0",
			[9] = "3",
			[10] = "0 destinations",
			[21] = "station-end-lan"
		};

		foreach (var (parameterNumber, expectedValue) in expectedValues)
		{
			using var status = await this.WaitForRouterParameterStatusAsync(
				this.participantParameterStatusAddresses![parameterNumber]);

			status.RootElement.GetProperty("parameterNumber").GetByte().Should().Be(parameterNumber);
			status.RootElement.GetProperty("parameterValue").GetString().Should().Be(expectedValue);
		}
	}

	[Then(@"the Participant Parameter Request statuses show the Printer UA Current values")]
	public async Task ThenTheParticipantParameterRequestStatusesShowThePrinterUaCurrentValues()
	{
		var expectedValues = new Dictionary<byte, string>
		{
			[1] = "2",
			[2] = "Printer (4)",
			[3] = "26.100.25",
			[21] = "26.100.0-26.100.63",
			[22] = "true",
			[23] = "0 entries",
			[24] = "false"
		};

		foreach (var (parameterNumber, expectedValue) in expectedValues)
		{
			using var status = await this.WaitForRouterParameterStatusAsync(
				this.participantParameterStatusAddresses![parameterNumber]);

			status.RootElement.GetProperty("parameterNumber").GetByte().Should().Be(parameterNumber);
			status.RootElement.GetProperty("parameterValue").GetString().Should().Be(expectedValue);
		}
	}

	[Then(@"the Participant Parameter Request statuses show the Network Management UA Current values")]
	public async Task ThenTheParticipantParameterRequestStatusesShowTheNetworkManagementUaCurrentValues()
	{
		var expectedValues = new Dictionary<byte, string>
		{
			[1] = "25",
			[2] = "Network Management UA (12)",
			[3] = "26.100.25"
		};

		foreach (var (parameterNumber, expectedValue) in expectedValues)
		{
			using var status = await this.WaitForRouterParameterStatusAsync(
				this.participantParameterStatusAddresses![parameterNumber]);

			status.RootElement.GetProperty("parameterNumber").GetByte().Should().Be(parameterNumber);
			status.RootElement.GetProperty("parameterValue").GetString().Should().Be(expectedValue);
		}
	}

	[Then(@"the Node Login status eventually shows User-Agent address (.*)\.(.*)\.(.*) is logged on")]
	public async Task ThenTheNodeLoginStatusEventuallyShowsUserAgentAddressIsLoggedOn(
		byte brigade,
		ushort node,
		byte port)
	{
		var statusAddress = this.response!.Headers.Location!;
		var expectedAddress = $"{brigade}.{node}.{port}";
		string? lastStatus = null;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = await this.client!.GetAsync(statusAddress);
			var content = await status.Content.ReadAsStringAsync();
			lastStatus = $"{status.StatusCode}: {content}";

			if (status.StatusCode == HttpStatusCode.OK &&
				content.Contains("logged-on", StringComparison.Ordinal) &&
				content.Contains(expectedAddress, StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			$"The Node Login status did not show User-Agent address {expectedAddress} as logged on. " +
			$"Last status: {lastStatus}");
	}

	[Then(@"the Node Login status eventually shows invalid password")]
	public async Task ThenTheNodeLoginStatusEventuallyShowsInvalidPassword()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = await this.client!.GetAsync(statusAddress);
			var content = await status.Content.ReadAsStringAsync();

			if (status.StatusCode == HttpStatusCode.OK &&
				content.Contains("invalid_password", StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Node Login status did not show an invalid password rejection.");
	}

	[Then(@"the Node Login status eventually shows the Router is logged off")]
	public async Task ThenTheNodeLoginStatusEventuallyShowsTheRouterIsLoggedOff()
	{
		var statusAddress = this.response!.Headers.Location!;

		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = await this.client!.GetAsync(statusAddress);
			var content = await status.Content.ReadAsStringAsync();

			if (status.StatusCode == HttpStatusCode.OK &&
				content.Contains("logged-off", StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException("The Node Login status did not show that the Router is logged off.");
	}

	[Then(@"the Router retains brigade or agency number (.*) in its permanent and non-volatile Parameter Tables")]
	public async Task ThenTheRouterRetainsBrigadeOrAgencyNumberInItsPersistentParameterTables(
		byte brigadeOrAgencyNumber)
	{
		var connectionString = await this.application!.GetConnectionStringAsync("router-database");
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();
		await using var command = new NpgsqlCommand(
			"""
			SELECT COUNT(*)
			FROM node.parameter_value AS parameter_value
			INNER JOIN node.parameter_set AS parameter_set
				ON parameter_set."Id" = parameter_value."ParameterSetId"
			WHERE parameter_value."ParameterNumber" = 1
				AND parameter_set."Kind" IN (0, 1)
				AND parameter_value."EncodedValue" = @encodedValue;
			""",
			connection);
		command.Parameters.AddWithValue("encodedValue", new byte[] { brigadeOrAgencyNumber });

		(await command.ExecuteScalarAsync()).Should().Be(2L);
	}

	[AfterScenario]
	public void DisposeClient()
	{
		this.client?.Dispose();
	}

	[AfterFeature]
	public static async Task DisposeApplicationsAsync()
	{
		await applications.DisposeAsync();
	}

	private async Task EnsureApplicationStartedAsync()
	{
		if (this.application is not null)
		{
			return;
		}

		this.application = await applications.GetAsync(
			this.applicationProfile,
			StartApplicationAsync);
		this.client = CreateClient(this.application);
	}

	private static async Task<DistributedApplication> StartApplicationAsync(
		RouterParameterRequestApplicationProfile applicationProfile)
	{
		var localRouterDoesNotRespond =
			applicationProfile == RouterParameterRequestApplicationProfile.NonrespondingRouter;
		var isHighConcurrencyInventoryScanProfile =
			applicationProfile == RouterParameterRequestApplicationProfile.HighConcurrencyInventoryScan;
		var appHost = await DistributedApplicationTestingBuilder
			.CreateAsync<Projects.GD92_StationEnd_AppHost>(
				[
					"--Persistence:UsePersistentPostgres=false",
					"--StationEnd:IncludeBusMTAAndIOUA=false",
					localRouterDoesNotRespond
						? "--RouterParameterRequest:LocalRouter:Port=63"
						: "--RouterParameterRequest:LocalRouter:Port=0",
					localRouterDoesNotRespond
						? "--GD92:no_ack_timeout=1"
						: "--GD92:no_ack_timeout=5",
					localRouterDoesNotRespond
						? "--GD92:retries=1"
						: "--GD92:retries=3",
					isHighConcurrencyInventoryScanProfile
						? "--InventoryScan:MaximumConcurrentProbes=12"
						: "--InventoryScan:MaximumConcurrentProbes=8",
					"--Parameters:router-level1-password=FIRE1"
					, "--Parameters:router-level2-password=TESTL2",
					"--Parameters:router-level3-password=TESTL3",
					"--Parameters:router-level4-password=TESTL4"
				]);

		var application = await appHost.BuildAsync();
		await application.StartAsync();
		await StationEndApplicationStartup.WaitForHealthyAsync(
			application,
			"Router",
			"LAN-MTA",
			"Printer-UA",
			"Node-Manager-UA");
		var rabbitMqConnectionString = await application.GetConnectionStringAsync("RabbitMQ")
			?? throw new InvalidOperationException(
				"The test RabbitMQ connection string was not provided.");
		await WaitForQueueConsumerAsync(
			rabbitMqConnectionString,
			"gd92.participant.26.100.1",
			"LAN MTA local participant ingress");
		return application;
	}

	private async Task<JsonDocument> WaitForInventoryScanStatusAsync(
		Func<JsonElement, bool> condition)
	{
		for (var attempt = 0; attempt < 150; attempt++)
		{
			var response = await this.client!.GetAsync(this.inventoryScanStatusAddress!);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var content = await response.Content.ReadAsStringAsync();
				using var status = JsonDocument.Parse(content);
				if (condition(status.RootElement))
				{
					return JsonDocument.Parse(content);
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Inventory Scan status did not reach the expected state.");
	}

	private async Task<JsonDocument> WaitForRouterParameterStatusAsync(Uri statusAddress)
	{
		for (var attempt = 0; attempt < 30; attempt++)
		{
			var response = await this.client!.GetAsync(statusAddress);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var content = await response.Content.ReadAsStringAsync();
				using var status = JsonDocument.Parse(content);
				if (status.RootElement.GetProperty("state").GetString() == "received")
				{
					return JsonDocument.Parse(content);
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			"The Router Parameter Request did not receive a Parameter Value.");
	}

	private static HttpClient CreateClient(DistributedApplication application)
	{
		return new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false
		})
		{
			BaseAddress = application.GetEndpoint("Node-Manager-UA")
		};
	}

	private static HttpClient CreateRedirectFollowingClient(DistributedApplication application)
	{
		return new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = true
		})
		{
			BaseAddress = application.GetEndpoint("Node-Manager-UA")
		};
	}

	private async Task<string> GetResourceLogAsync(string resourceName)
	{
		if (this.application is null)
		{
			return string.Empty;
		}

		var resourceLogger = this.application.Services.GetRequiredService<ResourceLoggerService>();
		var lines = new List<string>();
		await foreach (var batch in resourceLogger.GetAllAsync(resourceName))
		{
			lines.AddRange(batch.Select(line => line.Content));
		}

		return lines.Count == 0
			? string.Empty
			: $"{Environment.NewLine}{resourceName} log:{Environment.NewLine}" +
				string.Join(Environment.NewLine, lines.TakeLast(50));
	}

	private static async Task WaitForQueueConsumerAsync(
		string rabbitMqConnectionString,
		string queueName,
		string ingressName)
	{
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var factory = new ConnectionFactory
		{
			Uri = new Uri(rabbitMqConnectionString)
		};

		while (!timeout.IsCancellationRequested)
		{
			try
			{
				await using var connection = await factory.CreateConnectionAsync(timeout.Token);
				await using var channel = await connection.CreateChannelAsync(
					cancellationToken: timeout.Token);
				var queue = await channel.QueueDeclarePassiveAsync(
					queueName,
					cancellationToken: timeout.Token);
				if (queue.ConsumerCount > 0)
				{
					return;
				}
			}
			catch (OperationInterruptedException)
			{
				// The listener has not declared its queue yet.
			}

			try
			{
				await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token);
			}
			catch (OperationCanceledException) when (timeout.IsCancellationRequested)
			{
				break;
			}
		}

		throw new Xunit.Sdk.XunitException(
			$"{ingressName} did not start a RabbitMQ consumer within 30 seconds.");
	}

	private enum RouterParameterRequestApplicationProfile
	{
		Default,
		NonrespondingRouter,
		HighConcurrencyInventoryScan,
		FreshParameterTables,
		RouterWithRoutingTableEntry
	}

	private sealed class RouterParameterRequestApplicationPool : IAsyncDisposable
	{
		private readonly Dictionary<RouterParameterRequestApplicationProfile, DistributedApplication>
			applications = [];
		private readonly SemaphoreSlim gate = new(1, 1);

		public async Task<DistributedApplication> GetAsync(
			RouterParameterRequestApplicationProfile applicationProfile,
			Func<RouterParameterRequestApplicationProfile, Task<DistributedApplication>> createApplication)
		{
			await this.gate.WaitAsync();
			try
			{
				if (this.applications.TryGetValue(applicationProfile, out var application))
				{
					return application;
				}

				foreach (var existingApplication in this.applications.ToArray())
				{
					await existingApplication.Value.StopAsync();
					await existingApplication.Value.DisposeAsync();
					this.applications.Remove(existingApplication.Key);
				}

				application = await createApplication(applicationProfile);
				this.applications.Add(applicationProfile, application);
				return application;
			}
			finally
			{
				this.gate.Release();
			}
		}

		public async ValueTask DisposeAsync()
		{
			await this.gate.WaitAsync();
			try
			{
				foreach (var application in this.applications.Values)
				{
					await application.StopAsync();
					await application.DisposeAsync();
				}

				this.applications.Clear();
			}
			finally
			{
				this.gate.Release();
			}
		}
	}
}
