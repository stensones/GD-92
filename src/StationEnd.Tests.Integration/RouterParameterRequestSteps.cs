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

		var client = new HttpClient
		{
			BaseAddress = this.application.GetEndpoint("Node-Manager-UA")
		};

		this.response = await client.PostAsync("/router/parameters/brigade-or-agency-number", null);
	}

	[Then(@"I am redirected to the pending Parameter Request status")]
	public void ThenIAmRedirectedToThePendingParameterRequestStatus()
	{
		this.response!.StatusCode.Should().Be(HttpStatusCode.SeeOther);
		this.response.Headers.Location.Should().NotBeNull();
	}

	[AfterScenario]
	public async Task DisposeApplication()
	{
		if (this.application is not null)
		{
			await this.application.DisposeAsync();
		}
	}
}
