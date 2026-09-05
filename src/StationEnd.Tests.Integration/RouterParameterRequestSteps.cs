using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Npgsql;
using Reqnroll;
using System.Net;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class RouterParameterRequestSteps
{
	private DistributedApplication? application;
	private HttpClient? client;
	private HttpResponseMessage? response;
	private string? level1Password;
	private string? pageContent;

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

	[When(@"I request the local Router brigade or agency number")]
	public async Task WhenIRequestTheLocalRouterBrigadeOrAgencyNumber()
	{
		if (this.application is null)
		{
			await this.StartApplicationAsync();
		}

		this.response = await this.client!.PostAsync("/router/parameters/brigade-or-agency-number", null);
	}

	[When(@"I open NodeManager")]
	public async Task WhenIOpenNodeManager()
	{
		if (this.application is null)
		{
			await this.StartApplicationAsync();
		}

		this.response = await this.client!.GetAsync("/");
		this.response.EnsureSuccessStatusCode();
		this.pageContent = await this.response.Content.ReadAsStringAsync();
	}

	[Then(@"NodeManager presents a Node Login form that securely submits password, brigade, node, and port")]
	public void ThenNodeManagerPresentsANodeLoginForm()
	{
		this.pageContent.Should().Contain("""<form id="router-logon" action="/router/parameters/logon" method="post">""");
		this.pageContent.Should().Contain("""type="password" name="password" required""");
		this.pageContent.Should().Contain("""type="number" name="brigade" required""");
		this.pageContent.Should().Contain("""type="number" name="node" required""");
		this.pageContent.Should().Contain("""type="number" name="port" required""");
		this.pageContent.Should().Contain("""type="submit">Log on</button>""");
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
		if (this.application is null)
		{
			await this.StartApplicationAsync();
		}

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
		await this.StartApplicationAsync();
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

	[Then(@"the Node Login status eventually shows User-Agent address (.*)\.(.*)\.(.*) is logged on")]
	public async Task ThenTheNodeLoginStatusEventuallyShowsUserAgentAddressIsLoggedOn(
		byte brigade,
		ushort node,
		byte port)
	{
		var statusAddress = this.response!.Headers.Location!;
		var expectedAddress = $"{brigade}.{node}.{port}";

		for (var attempt = 0; attempt < 30; attempt++)
		{
			var status = await this.client!.GetAsync(statusAddress);
			var content = await status.Content.ReadAsStringAsync();

			if (status.StatusCode == HttpStatusCode.OK &&
				content.Contains("logged-on", StringComparison.Ordinal) &&
				content.Contains(expectedAddress, StringComparison.Ordinal))
			{
				return;
			}

			await Task.Delay(TimeSpan.FromSeconds(1));
		}

		throw new Xunit.Sdk.XunitException(
			$"The Node Login status did not show User-Agent address {expectedAddress} as logged on.");
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
	public async Task DisposeApplication()
	{
		this.client?.Dispose();

		if (this.application is not null)
		{
			await this.application.DisposeAsync();
		}
	}

	private async Task StartApplicationAsync()
	{
		var appHost = await DistributedApplicationTestingBuilder
			.CreateAsync<Projects.GD92_StationEnd_AppHost>();
		appHost.Configuration["Persistence:UsePersistentPostgres"] = "false";
		appHost.Configuration["Parameters:router-level1-password"] = this.level1Password ?? "FIRE1";

		this.application = await appHost.BuildAsync();
		await this.application.StartAsync();
		this.client = CreateClient(this.application);
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
}
