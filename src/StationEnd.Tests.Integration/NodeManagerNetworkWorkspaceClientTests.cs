using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Stensones.GD92.StationEnd.Tests.Integration;

public sealed class NodeManagerNetworkWorkspaceClientTests
{
	[Fact]
	public async Task Renders_Participant_Parameters_navigation_for_the_discovered_port_and_Agent_Type()
	{
		using var application = new NodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/network?address=26.100.0");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain("<th scope=\"col\">Parameters</th>");
		content.Should().Contain(
			"const participantCommunicationsAddress = port => " +
			"`${selectedCommunicationsAddress.substring(0, selectedCommunicationsAddress.lastIndexOf(\".\") + 1)}${port}`;");
		content.Should().Contain("if (participant.agentType)");
		content.Should().Contain(
			"`/parameters?address=${encodeURIComponent(participantCommunicationsAddress(participant.port))}" +
			"&agentType=${encodeURIComponent(participant.agentType)}`");
		content.Should().Contain("parametersLink.textContent = \"View Parameters\";");
	}

	private sealed class NodeManagerApplicationFactory : WebApplicationFactory<NodeManagerApplication>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Testing");
		}
	}
}
