using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodeManager.Router.Parameters;

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
		content.Should().Contain("const tableEntries = result.routingTableEntries ?? [];");
		content.Should().Contain("const hasMoreValues = result.moreValues === true;");
		content.Should().Contain("tableEntries.at(-1).index + 1");
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

	[Fact]
	public async Task Renders_an_authorized_timeout_editor_that_reveals_only_for_a_valid_received_value()
	{
		using var application = new AuthorizedNodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/parameters?address=26.100.0&agentType=Router&selectedParameter=12");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain("<form id=\"no-acknowledgement-timeout-edit\" hidden>");
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-new-value\" type=\"number\" min=\"1\" max=\"255\"");
		content.Should().Contain(
			"if (result.state === \"received\" && result.parameterValue !== null)");
		content.Should().Contain("typeof value === \"string\" &&");
		content.Should().Contain(
			"/^(?:[1-9]|[1-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])$/.test(value)");
		content.Should().Contain(
			"!isCanonicalNoAcknowledgementTimeoutValue.test(result.parameterValue)");
		content.Should().Contain(
			"noAcknowledgementTimeoutCurrentValue.value = result.parameterValue;");
		content.Should().Contain("noAcknowledgementTimeoutEdit.hidden = false;");
		content.Should().Contain(
			"Received an invalid No Acknowledgement Timeout value.");
		content.Should().NotContain(
			"managementTransactions.submit(noAcknowledgementTimeoutEdit.action");
	}

	[Fact]
	public async Task Renders_a_local_timeout_change_review_with_canonical_value_validation()
	{
		using var application = new AuthorizedNodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/parameters?address=26.100.0&agentType=Router&selectedParameter=12");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-new-value\" type=\"number\" min=\"1\" max=\"255\"");
		content.Should().Contain(
			"<button id=\"no-acknowledgement-timeout-review-submit\" type=\"submit\">Review change</button>");
		content.Should().Contain(
			"<section id=\"no-acknowledgement-timeout-review\" hidden");
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-review-selected-communications-address\"");
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-review-parameter-table\"");
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-review-prior-value\"");
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-review-new-value\"");
		content.Should().Contain(
			"<button id=\"no-acknowledgement-timeout-edit-change\" type=\"button\">Edit change</button>");
		content.Should().Contain(
			"noAcknowledgementTimeoutNewValue.setCustomValidity(");
		content.Should().Contain(
			"noAcknowledgementTimeoutNewValue.reportValidity();");
		content.Should().Contain(
			"noAcknowledgementTimeoutReview.hidden = false;");
		content.Should().Contain(
			"noAcknowledgementTimeoutEdit.hidden = false;");
		content.Should().Contain(
			"Review ready. Check the proposed Parameter change.");
		content.Should().NotContain(
			"managementTransactions.submit(noAcknowledgementTimeoutEdit.action");
	}

	[Fact]
	public async Task Renders_a_reviewed_timeout_modification_submission()
	{
		using var application = new AuthorizedNodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync("/parameters?address=26.100.0&agentType=Router&selectedParameter=12");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain(
			"<form id=\"no-acknowledgement-timeout-modification\" action=\"/router/parameters/current/12/value\" method=\"post\" hidden>");
		content.Should().Contain(
			"id=\"no-acknowledgement-timeout-reviewed-value\" name=\"value\" type=\"hidden\"");
		content.Should().Contain(
			"managementTransactions.submit(noAcknowledgementTimeoutModification.action, { method: \"POST\", body: new FormData(noAcknowledgementTimeoutModification) })");
		content.Should().Contain(
			"Pending, Acknowledged, Rejected, Timed out, Delivery failed.");
	}

	[Fact]
	public async Task Renders_a_reviewed_timeout_modification_for_the_selected_Parameter_Table()
	{
		using var application = new AuthorizedNodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync(
				"/parameters?address=26.100.0&agentType=Router&parameterTable=non-volatile&selectedParameter=12");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain(
			"<form id=\"no-acknowledgement-timeout-modification\" action=\"/router/parameters/non-volatile/12/value\" method=\"post\" hidden>");
	}

	[Theory]
	[InlineData(13, "Routing Table", "routingTableEntries", "Entry index|Next node", "index|nextNode")]
	[InlineData(14, "PSTN Table", "pstnTableEntries", "Entry index|Used|Next node|Telephone number|Hold time|Available", "index|used|nextNode|telephoneNumber|holdTime|available")]
	[InlineData(15, "WAN Table", "wanTableEntries", "Entry index|Used|Next node|WAN address|Connect type", "index|used|nextNode|wanAddress|connectType")]
	[InlineData(16, "LAN Table", "lanTableEntries", "Entry index|Used|Next node|LAN address", "index|used|nextNode|lanAddress")]
	[InlineData(17, "ISDN Table", "isdnTableEntries", "Entry index|Used|Next node|Telephone number|Hold time|Available", "index|used|nextNode|telephoneNumber|holdTime|available")]
	[InlineData(21, "MDT Table", "mdtTableEntries", "Entry index|Used|Next node|Network user address|Hold time|Available", "index|used|nextNode|networkUserAddress|holdTime|available")]
	public async Task Renders_each_selected_Router_table_from_its_corresponding_response_field(
		byte parameterNumber,
		string tableName,
		string responseField,
		string columnHeaders,
		string valueProperties)
	{
		using var application = new NodeManagerApplicationFactory();
		using var response = await application.CreateClient()
			.GetAsync(
				$"/parameters?address=26.100.0&agentType=Router&selectedTableParameter={parameterNumber}");
		var content = await response.Content.ReadAsStringAsync();

		response.EnsureSuccessStatusCode();
		content.Should().Contain($"<caption>Current Router {tableName} entries</caption>");
		content.Should().Contain($"const tableEntries = result.{responseField} ?? [];");
		content.Should().Contain(
			$"href=\"/parameters?address=26.100.0&amp;agentType=Router&amp;parameterTable=current&amp;selectedTableParameter={parameterNumber}\"");
		foreach (var columnHeader in columnHeaders.Split('|'))
		{
			content.Should().Contain($"<th scope=\"col\">{columnHeader}</th>");
		}
		foreach (var valueProperty in valueProperties.Split('|'))
		{
			content.Should().Contain(
				$"{valueProperty}.textContent = entry.{valueProperty};");
		}
	}

	private class NodeManagerApplicationFactory : WebApplicationFactory<NodeManagerApplication>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Testing");
		}
	}

	private sealed class AuthorizedNodeManagerApplicationFactory : NodeManagerApplicationFactory
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			base.ConfigureWebHost(builder);
			builder.ConfigureServices(services =>
			{
				services.RemoveAll<IRouterSessionAuthorization>();
				services.AddSingleton<IRouterSessionAuthorization>(
					new AuthorizedRouterSessionAuthorization());
			});
		}
	}

	private sealed class AuthorizedRouterSessionAuthorization : IRouterSessionAuthorization
	{
		public bool IsAuthorized(string browserSessionIdentifier) => true;

		public void TrackLogOn(
			string browserSessionIdentifier,
			RouterParameterRequestStatusIdentifier transactionIdentifier)
		{
		}

		public void TrackLogOff(
			string browserSessionIdentifier,
			RouterParameterRequestStatusIdentifier transactionIdentifier)
		{
		}

		public void Observe(
			string browserSessionIdentifier,
			RouterParameterRequestStatusIdentifier transactionIdentifier,
			RouterParameterRequestStatus status)
		{
		}
	}
}
