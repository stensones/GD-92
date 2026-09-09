using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Npgsql;
using Reqnroll;
using System.Net;
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

	[When(@"I request the local Router brigade or agency number")]
	public async Task WhenIRequestTheLocalRouterBrigadeOrAgencyNumber()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/parameters/brigade-or-agency-number", null);
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
		this.pageContent.Should().Contain("""<form id="router-logon" action="/router/parameters/logon" method="post">""");
		this.pageContent.Should().Contain("""type="password" name="password" required""");
		this.pageContent.Should().Contain("""type="number" name="brigade""");
		this.pageContent.Should().Contain("""type="number" name="node""");
		this.pageContent.Should().Contain("""type="number" name="port""");
		this.pageContent.Should().Contain("""type="submit">Log on</button>""");
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
		this.pageContent.Should().Contain(
			"""<form id="router-participant-discovery" action="/router/participants/discovery" method="post">""");
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

	[When(@"I select Discover local participants")]
	public async Task WhenISelectDiscoverLocalParticipants()
	{
		await this.EnsureApplicationStartedAsync();

		this.response = await this.client!.PostAsync("/router/participants/discovery", null);
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
				]);

		var application = await appHost.BuildAsync();
		await application.StartAsync();
		await application.ResourceNotifications.WaitForResourceHealthyAsync("Router");
		await application.ResourceNotifications.WaitForResourceHealthyAsync("Node-Manager-UA");
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

	private enum RouterParameterRequestApplicationProfile
	{
		Default,
		NonrespondingRouter,
		HighConcurrencyInventoryScan,
		FreshParameterTables
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
