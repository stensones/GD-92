using AwesomeAssertions;
using Router.Persistence;
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
	public void Provides_initial_passwords_for_all_four_levels()
	{
		var passwords = Enumerable.Range(0, 4)
			.Select(_ => Guid.NewGuid().ToString("N")[..10])
			.ToArray();
		passwords.Should().OnlyHaveUniqueItems();
		var configuration = CreateConfiguration(
			("Router:InitialLevel1Password", passwords[0]),
			("Router:InitialLevel2Password", passwords[1]),
			("Router:InitialLevel3Password", passwords[2]),
			("Router:InitialLevel4Password", passwords[3]));

		var settings = RouterSettings.FromConfiguration(configuration);

		new[]
		{
			settings.InitialLevel1Password,
			settings.InitialLevel2Password,
			settings.InitialLevel3Password,
			settings.InitialLevel4Password
		}.Should().Equal(passwords.Select(password =>
			PasswordValue.FromValue(SevenBitAsciiString.FromValue(password))));
	}

	[Fact]
	public void Provides_typed_initial_no_acknowledgement_timeout_and_retries()
	{
		var configuration = CreateConfiguration(
			("Router:InitialLevel1Password", "FIRE"),
			("Router:NoAcknowledgementTimeout", "8"),
			("Router:Retries", "7"));

		var settings = RouterSettings.FromConfiguration(configuration);

		settings.NoAcknowledgementTimeout.Should().Be(
			NoAcknowledgementTimeout.FromValue(Word8.FromValue(8)));
		settings.Retries.Should().Be(Retries.FromValue(Word8.FromValue(7)));
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
			["Router:NodeName"] = "Station End",
			["Router:MaximumMessageLength"] = "1023",
			["Router:NetworkManagerAddress1:Brigade"] = "26",
			["Router:NetworkManagerAddress1:Node"] = "100",
			["Router:NetworkManagerAddress1:Port"] = "25",
			["Router:NetworkManagerAddress2:Brigade"] = "26",
			["Router:NetworkManagerAddress2:Node"] = "100",
			["Router:NetworkManagerAddress2:Port"] = "25",
			["Router:ProtocolVersion"] = "2",
			["Router:InitialLevel2Password"] = Guid.NewGuid().ToString("N")[..10],
			["Router:InitialLevel3Password"] = Guid.NewGuid().ToString("N")[..10],
			["Router:InitialLevel4Password"] = Guid.NewGuid().ToString("N")[..10],
			["Router:NoAcknowledgementTimeout"] = "5",
			["Router:Retries"] = "3",
			["Router:ManualAcknowledgementTimeout"] = "60"
		};

		foreach (var value in values)
		{
			configurationValues[value.Key] = value.Value;
		}

		return new ConfigurationBuilder()
			.AddInMemoryCollection(configurationValues)
			.Build();
	}
}
