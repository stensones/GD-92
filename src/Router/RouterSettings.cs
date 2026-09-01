using Microsoft.Extensions.Configuration;
using Stensones.GD92.Fields;

namespace Router;

internal sealed class RouterSettings
{
	private RouterSettings(CommunicationsAddress localAddress, ProtocolVersion protocolVersion)
	{
		this.LocalAddress = localAddress;
		this.ProtocolVersion = protocolVersion;
	}

	public CommunicationsAddress LocalAddress { get; }
	public ProtocolVersion ProtocolVersion { get; }

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
				ProtocolVersionNumber.FromValue(routerConfiguration.GetValue<byte>("ProtocolVersion"))));
	}
}
