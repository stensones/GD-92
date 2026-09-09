using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using NodeManager.Router.Participants;

namespace NodeManager.Tests.Unit.Router.Participants;

public sealed class InventoryScanSettingsTests
{
	[Fact]
	public void Falls_back_to_eight_concurrent_probes()
	{
		var settings = InventoryScanSettings.FromConfiguration(Configuration());

		settings.MaximumConcurrentProbes.Should().Be(8);
	}

	[Fact]
	public void Uses_the_configured_maximum_concurrent_probes()
	{
		var settings = InventoryScanSettings.FromConfiguration(
			Configuration(("InventoryScan:MaximumConcurrentProbes", "63")));

		settings.MaximumConcurrentProbes.Should().Be(63);
	}

	[Theory]
	[InlineData("0")]
	[InlineData("64")]
	[InlineData("invalid")]
	public void Rejects_an_invalid_maximum_concurrent_probes(string maximumConcurrentProbes)
	{
		var createSettings = () => InventoryScanSettings.FromConfiguration(
			Configuration(("InventoryScan:MaximumConcurrentProbes", maximumConcurrentProbes)));

		createSettings.Should().Throw<InvalidOperationException>()
			.WithMessage("*InventoryScan:MaximumConcurrentProbes*");
	}

	private static IConfiguration Configuration(params (string Key, string Value)[] values)
	{
		return new ConfigurationBuilder()
			.AddInMemoryCollection(values.ToDictionary(
				value => value.Key,
				value => (string?)value.Value))
			.Build();
	}
}
