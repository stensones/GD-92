using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Stensones.GD92.StationEnd.Tests.Integration;

public sealed class NodeManagerParametersWorkspaceClientTests
{
	[Fact]
	public async Task Renders_a_transaction_backed_Current_brigade_or_agency_request()
	{
		using var application = new NodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/parameters?address=26.100.0&agentType=Router");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain("/js/management-transactions.js");
		content.Should().Contain(
			"managementTransactions.submit(parameterRequestForm.action, { method: \"POST\" })");
		content.Should().Contain(
			"<td id=\"brigade-or-agency-read-state\">Not read</td>");
		content.Should().Contain(
			"<td id=\"brigade-or-agency-last-read\"></td>");
		content.Should().Contain(
			"brigadeOrAgencyReadState.textContent = \"Pending\"");
		content.Should().Contain(
			"brigadeOrAgencyLastRead.textContent = new Date().toLocaleString()");
	}

	[Fact]
	public async Task Renders_a_transaction_backed_Current_Routing_Table_request_with_received_entry_paging()
	{
		using var application = new NodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/parameters?address=26.100.0&agentType=Router");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain(
			"managementTransactions.submit(routingTableRequestForm.action, { method: \"POST\" })");
		content.Should().Contain("const entries = result.routingTableEntries ?? [];");
		content.Should().Contain("const hasMoreValues = result.moreValues === true;");
		content.Should().Contain("entries.at(-1).index + 1");
		content.Should().Contain("routingTableNextEntries.disabled = !hasNextEntries;");
	}

	[Fact]
	public async Task Renders_a_Parameter_Table_selector_that_navigates_with_the_selected_table()
	{
		using var application = new NodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/parameters?address=26.100.0&agentType=Router&parameterTable=unsupported");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain("<option selected>Current</option>");
		content.Should().Contain(
			"parameterTableSelector.addEventListener(\"change\", () => {");
		content.Should().Contain("url.searchParams.set(\"parameterTable\"");
		content.Should().Contain("window.location.assign(url);");
	}

	private sealed class NodeManagerApplicationFactory : WebApplicationFactory<NodeManagerApplication>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Testing");
		}
	}
}
