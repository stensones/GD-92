using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Stensones.GD92.Fields;

namespace Router.Tests.Unit;

public sealed class RouterSettingsTests
{
	[Fact]
	public void Provides_a_nonempty_ten_character_7_bit_ASCII_initial_level_one_password()
	{
		var configuration = CreateConfiguration(("Router:InitialLevel1Password", "0123456789"));

		var settings = RouterSettings.FromConfiguration(configuration);

		settings.InitialLevel1Password.Should().Be(
			PasswordValue.FromValue(SevenBitAsciiString.FromValue("0123456789")));
	}

	[Fact]
	public void Rejects_a_missing_initial_level_one_password()
	{
		var configuration = CreateConfiguration();

		var createSettings = () => RouterSettings.FromConfiguration(configuration);

		createSettings.Should().Throw<InvalidOperationException>()
			.WithMessage("*Router:InitialLevel1Password*");
	}

	[Theory]
	[InlineData("")]
	[InlineData("01234567890")]
	[InlineData("FIRÉ")]
	public void Rejects_an_invalid_initial_level_one_password(string password)
	{
		var configuration = CreateConfiguration(("Router:InitialLevel1Password", password));

		var createSettings = () => RouterSettings.FromConfiguration(configuration);

		createSettings.Should().Throw<InvalidOperationException>()
			.WithMessage("*Router:InitialLevel1Password*");
	}

	private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
	{
		var configurationValues = new Dictionary<string, string?>
		{
			["Router:LocalAddress:Brigade"] = "26",
			["Router:LocalAddress:Node"] = "100",
			["Router:LocalAddress:Port"] = "0",
			["Router:ProtocolVersion"] = "2"
		};

		foreach (var value in values)
		{
			configurationValues.Add(value.Key, value.Value);
		}

		return new ConfigurationBuilder()
			.AddInMemoryCollection(configurationValues)
			.Build();
	}
}
