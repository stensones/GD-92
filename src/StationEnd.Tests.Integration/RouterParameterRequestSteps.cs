using Aspire.Hosting;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using Reqnroll;
using System.Net;

namespace Stensones.GD92.StationEnd.Tests.Integration;

[Binding]
public sealed class RouterParameterRequestSteps
{
	private DistributedApplication? application;
	private HttpClient? client;
	private HttpResponseMessage? response;

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
		var appHost = await DistributedApplicationTestingBuilder
			.CreateAsync<Projects.GD92_StationEnd_AppHost>();

		this.application = await appHost.BuildAsync();
		await this.application.StartAsync();

		this.client = new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false
		})
		{
			BaseAddress = this.application.GetEndpoint("Node-Manager-UA")
		};

		this.response = await this.client.PostAsync("/router/parameters/brigade-or-agency-number", null);
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

	[AfterScenario]
	public async Task DisposeApplication()
	{
		this.client?.Dispose();

		if (this.application is not null)
		{
			await this.application.DisposeAsync();
		}
	}
}
