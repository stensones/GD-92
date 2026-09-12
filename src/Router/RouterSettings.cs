using Microsoft.Extensions.Configuration;
using Router.Persistence;
using Stensones.GD92.Fields;
using System.Globalization;

namespace Router;

internal sealed class RouterSettings
{
	private RouterSettings(
		CommunicationsAddress localAddress,
		NodeName nodeName,
		MaximumMessageLength maximumMessageLength,
		CommunicationsAddress networkManagerAddress1,
		CommunicationsAddress networkManagerAddress2,
		ProtocolVersion protocolVersion,
		PasswordValue initialLevel1Password,
		NoAcknowledgementTimeout noAcknowledgementTimeout,
		Retries retries,
		ManualAcknowledgementTimeout manualAcknowledgementTimeout,
		TimeAndDate timeAndDate)
	{
		this.LocalAddress = localAddress;
		this.NodeName = nodeName;
		this.MaximumMessageLength = maximumMessageLength;
		this.NetworkManagerAddress1 = networkManagerAddress1;
		this.NetworkManagerAddress2 = networkManagerAddress2;
		this.ProtocolVersion = protocolVersion;
		this.InitialLevel1Password = initialLevel1Password;
		this.NoAcknowledgementTimeout = noAcknowledgementTimeout;
		this.Retries = retries;
		this.ManualAcknowledgementTimeout = manualAcknowledgementTimeout;
		this.TimeAndDate = timeAndDate;
		this.ParameterBootstrapConfiguration = RouterParameterBootstrapConfiguration.FromValues(
			localAddress,
			nodeName,
			maximumMessageLength,
			networkManagerAddress1,
			networkManagerAddress2,
			initialLevel1Password,
			noAcknowledgementTimeout,
			retries,
			manualAcknowledgementTimeout,
			timeAndDate,
			MdtTable.FromEntries());
	}

	public CommunicationsAddress LocalAddress { get; }
	public NodeName NodeName { get; }
	public MaximumMessageLength MaximumMessageLength { get; }
	public CommunicationsAddress NetworkManagerAddress1 { get; }
	public CommunicationsAddress NetworkManagerAddress2 { get; }
	public ProtocolVersion ProtocolVersion { get; }
	public PasswordValue InitialLevel1Password { get; }
	public NoAcknowledgementTimeout NoAcknowledgementTimeout { get; }
	public Retries Retries { get; }
	public ManualAcknowledgementTimeout ManualAcknowledgementTimeout { get; }
	public TimeAndDate TimeAndDate { get; }

	public RouterParameterBootstrapConfiguration ParameterBootstrapConfiguration { get; }

	public static RouterSettings FromConfiguration(IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var routerConfiguration = configuration.GetRequiredSection("Router");
		var addressConfiguration = routerConfiguration.GetRequiredSection("LocalAddress");

		return new RouterSettings(
			ParseCommunicationsAddress(addressConfiguration),
			ParseNodeName(routerConfiguration["NodeName"]),
			MaximumMessageLength.FromValue(ParseRequiredWord16(
				routerConfiguration,
				"MaximumMessageLength")),
			ParseCommunicationsAddress(routerConfiguration.GetRequiredSection("NetworkManagerAddress1")),
			ParseCommunicationsAddress(routerConfiguration.GetRequiredSection("NetworkManagerAddress2")),
			ProtocolVersion.FromValue(
				ProtocolVersionNumber.FromValue(routerConfiguration.GetValue<byte>("ProtocolVersion"))),
			ParseInitialLevel1Password(routerConfiguration["InitialLevel1Password"]),
			NoAcknowledgementTimeout.FromValue(ParseRequiredWord8(
				routerConfiguration,
				"NoAcknowledgementTimeout")),
			Retries.FromValue(ParseRequiredWord8(routerConfiguration, "Retries")),
			ManualAcknowledgementTimeout.FromValue(ParseRequiredWord16(
				routerConfiguration,
				"ManualAcknowledgementTimeout")),
			CreateUtcTimeAndDate(DateTime.UtcNow));
	}

	private static NodeName ParseNodeName(string? value)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw new InvalidOperationException(
				"Router:NodeName must be a nonempty 7-bit ASCII string of at most 20 characters.");
		}

		try
		{
			return NodeName.FromValue(SevenBitAsciiString.FromValue(value));
		}
		catch (ArgumentException exception)
		{
			throw new InvalidOperationException(
				"Router:NodeName must be a nonempty 7-bit ASCII string of at most 20 characters.",
				exception);
		}
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

	private static CommunicationsAddress ParseCommunicationsAddress(
		IConfigurationSection addressConfiguration)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(
				BrigadeOrAgencyIdentifier.FromValue(addressConfiguration.GetValue<byte>("Brigade"))),
			Node.FromValue(NodeIdentifier.FromValue(addressConfiguration.GetValue<ushort>("Node"))),
			Port.FromValue(PortIdentifier.FromValue(addressConfiguration.GetValue<byte>("Port"))));
	}

	private static ushort ParseRequiredWord16(
		IConfigurationSection routerConfiguration,
		string settingName)
	{
		if (!ushort.TryParse(routerConfiguration[settingName], out var value))
		{
			throw new InvalidOperationException(
				$"Router:{settingName} must be an unsigned 16-bit integer.");
		}

		return value;
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

	private static TimeAndDate CreateUtcTimeAndDate(DateTime utcNow)
	{
		return TimeAndDate.FromValue(SevenBitAsciiString.FromValue(
			utcNow.ToString("ddMMMyyHHmmss", CultureInfo.InvariantCulture).ToUpperInvariant()));
	}
}
