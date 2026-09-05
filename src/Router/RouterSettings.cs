using Microsoft.Extensions.Configuration;
using Router.Persistence;
using Stensones.GD92.Fields;

namespace Router;

internal sealed class RouterSettings
{
	private RouterSettings(
		CommunicationsAddress localAddress,
		ProtocolVersion protocolVersion,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries)
	{
		this.LocalAddress = localAddress;
		this.ProtocolVersion = protocolVersion;
		this.InitialLevel1Password = initialLevel1Password;
		this.NoAcknowledgementTimeout = noAcknowledgementTimeout;
		this.Retries = retries;
	}

	public CommunicationsAddress LocalAddress { get; }
	public ProtocolVersion ProtocolVersion { get; }
	public PasswordValue InitialLevel1Password { get; }
	public NoAcknowledgementTimeout NoAcknowledgementTimeout { get; }
	public Retries Retries { get; }

	public RouterParameterBootstrapConfiguration ParameterBootstrapConfiguration =>
		RouterParameterBootstrapConfiguration.FromValues(
			this.LocalAddress,
			this.InitialLevel1Password,
			this.NoAcknowledgementTimeout,
			this.Retries);

	public static RouterSettings FromConfiguration(IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var routerConfiguration = configuration.GetRequiredSection("Router");
		var addressConfiguration = routerConfiguration.GetRequiredSection("LocalAddress");

		return new RouterSettings(
			CommunicationsAddress.FromValues(
				Brigade.FromValue(
					BrigadeOrAgencyIdentifier.FromValue(addressConfiguration.GetValue<byte>("Brigade"))),
				Node.FromValue(NodeIdentifier.FromValue(addressConfiguration.GetValue<ushort>("Node"))),
				Port.FromValue(PortIdentifier.FromValue(addressConfiguration.GetValue<byte>("Port")))),
			ProtocolVersion.FromValue(
				ProtocolVersionNumber.FromValue(routerConfiguration.GetValue<byte>("ProtocolVersion"))),
			ParseInitialLevel1Password(routerConfiguration["InitialLevel1Password"]),
			NoAcknowledgementTimeout.FromValue(ParseRequiredWord8(
				routerConfiguration,
				"NoAcknowledgementTimeout")),
			Retries.FromValue(ParseRequiredWord8(routerConfiguration, "Retries")));
	}

	private static PasswordValue ParseInitialLevel1Password(string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new InvalidOperationException(
				"Router:InitialLevel1Password must be a nonempty 7-bit ASCII password of at most 10 characters.");
		}

		try
		{
			return PasswordValue.FromValue(SevenBitAsciiString.FromValue(value));
		}
		catch (ArgumentException exception)
		{
			throw new InvalidOperationException(
				"Router:InitialLevel1Password must be a nonempty 7-bit ASCII password of at most 10 characters.",
				exception);
		}
	}

	private static Word8 ParseRequiredWord8(
		IConfigurationSection routerConfiguration,
		string settingName)
	{
		if (!byte.TryParse(routerConfiguration[settingName], out var value))
		{
			throw new InvalidOperationException(
				$"Router:{settingName} must be an unsigned 8-bit integer.");
		}

		return Word8.FromValue(value);
	}
}
