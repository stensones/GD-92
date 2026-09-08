using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using NodeManager.Router.Parameters;
using Stensones.GD92.Fields;

namespace NodeManager.Tests.Unit;

public sealed class RouterParameterRequestSettingsTests
{
	[Fact]
	public void Falls_back_to_the_GD92_five_second_timeout_and_three_total_sends()
	{
		var settings = RouterParameterRequestSettings.FromConfiguration(CreateConfiguration());

		settings.ManagementTransactionRetryPolicy.NoAcknowledgementTimeout.Should().Be(
			ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(5)));
		settings.ManagementTransactionRetryPolicy.TotalSends.Should().Be(
			ManagementTransactionTotalSends.FromValue(Word8.FromValue(3)));
	}

	private static IConfiguration CreateConfiguration()
	{
		return new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["RouterParameterRequest:MessageOriginator:Brigade"] = "26",
				["RouterParameterRequest:MessageOriginator:Node"] = "100",
				["RouterParameterRequest:MessageOriginator:Port"] = "25",
				["RouterParameterRequest:LocalRouter:Brigade"] = "26",
				["RouterParameterRequest:LocalRouter:Node"] = "100",
				["RouterParameterRequest:LocalRouter:Port"] = "0"
			})
			.Build();
	}
}
