using Microsoft.Extensions.Configuration;
using Stensones.GD92.Fields;

namespace NodeManager.Router.Parameters;

public sealed record RouterParameterRequestSettings(
	CommunicationsAddress MessageOriginator,
	CommunicationsAddress LocalRouter,
	ManagementTransactionRetryPolicy ManagementTransactionRetryPolicy)
{
	public RouterParameterRequestSettings(
		CommunicationsAddress messageOriginator,
		CommunicationsAddress localRouter) :
		this(messageOriginator, localRouter, DefaultManagementTransactionRetryPolicy)
	{
	}

	public static ManagementTransactionRetryPolicy DefaultManagementTransactionRetryPolicy { get; } =
		ManagementTransactionRetryPolicy.FromValues(
			ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(5)),
			ManagementTransactionTotalSends.FromValue(Word8.FromValue(3)));

	public static RouterParameterRequestSettings FromConfiguration(IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		var requestConfiguration = configuration.GetRequiredSection("RouterParameterRequest");
		var gd92Configuration = configuration.GetSection("GD92");

		return new RouterParameterRequestSettings(
			CreateAddress(requestConfiguration.GetRequiredSection("MessageOriginator")),
			CreateAddress(requestConfiguration.GetRequiredSection("LocalRouter")),
			ManagementTransactionRetryPolicy.FromValues(
				ManagementTransactionNoAcknowledgementTimeout.FromValue(Word8.FromValue(
					ParseByteOrDefault(gd92Configuration["no_ack_timeout"], 5, "GD92:no_ack_timeout"))),
				ManagementTransactionTotalSends.FromValue(Word8.FromValue(
					ParseByteOrDefault(gd92Configuration["retries"], 3, "GD92:retries")))));
	}

	private static CommunicationsAddress CreateAddress(IConfigurationSection configuration)
	{
		return CommunicationsAddress.FromValues(
			Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(configuration.GetValue<byte>("Brigade"))),
			Node.FromValue(NodeIdentifier.FromValue(configuration.GetValue<ushort>("Node"))),
			Port.FromValue(PortIdentifier.FromValue(configuration.GetValue<byte>("Port"))));
	}

	private static byte ParseByteOrDefault(string? value, byte fallback, string settingName)
	{
		if (value is null)
		{
			return fallback;
		}

		if (!byte.TryParse(value, out var parsed))
		{
			throw new InvalidOperationException($"{settingName} must be an unsigned 8-bit integer.");
		}

		return parsed;
	}
}
