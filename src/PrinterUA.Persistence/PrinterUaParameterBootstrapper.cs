using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace PrinterUA.Persistence;

public sealed class PrinterUaParameterBootstrapper
{
	private readonly ParticipantParameterBootstrapper parameterBootstrapper;

	public PrinterUaParameterBootstrapper(IParticipantParameterStore store)
	{
		this.parameterBootstrapper = new ParticipantParameterBootstrapper(store);
	}

	public async ValueTask<PrinterUaCurrentParameterProjection> LoadCurrentParameterProjectionAsync(
		PrinterUaParameterBootstrapConfiguration configuration,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		return await this.parameterBootstrapper.InitializeAsync(
		[
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.PortNumber,
				ParameterValue.FromWireValue([configuration.LocalAddress.Port.Value])),
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.AgentType,
				ParameterValue.FromWireValue(
					AgentType.FromValue(AgentTypeValue.Printer).ToWireValue())),
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.ControlAddress,
				ParameterValue.FromWireValue(configuration.ControlAddress.ToWireValue())),
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.DefaultSource.Number,
				PrinterUaParameterCatalogue.DefaultSource.Encode(
					CreateLocalNodeAddressRange(configuration.LocalAddress))),
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.NotifyPrinterAvailable.Number,
				PrinterUaParameterCatalogue.NotifyPrinterAvailable.Encode(ProtocolBoolean.True)),
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.AlternativeSource.Number,
				PrinterUaParameterCatalogue.AlternativeSource.Encode(AddressTable.FromEntries())),
			ParameterBootstrapValue.FromValues(
				PrinterUaParameterCatalogue.ReprintMessage.Number,
				PrinterUaParameterCatalogue.ReprintMessage.Encode(ProtocolBoolean.False))
		],
		static (nonVolatileValues, _) => ValueTask.FromResult(
			PrinterUaCurrentParameterProjection.FromNonVolatileParameters(
				nonVolatileValues[PrinterUaParameterCatalogue.PortNumber],
				nonVolatileValues[PrinterUaParameterCatalogue.AgentType],
				nonVolatileValues[PrinterUaParameterCatalogue.ControlAddress],
				PrinterUaParameterCatalogue.DefaultSource.Read(
					nonVolatileValues[PrinterUaParameterCatalogue.DefaultSource.Number]),
				PrinterUaParameterCatalogue.NotifyPrinterAvailable.Read(
					nonVolatileValues[PrinterUaParameterCatalogue.NotifyPrinterAvailable.Number]),
				PrinterUaParameterCatalogue.AlternativeSource.Read(
					nonVolatileValues[PrinterUaParameterCatalogue.AlternativeSource.Number]),
				PrinterUaParameterCatalogue.ReprintMessage.Read(
					nonVolatileValues[PrinterUaParameterCatalogue.ReprintMessage.Number]))),
		cancellationToken);
	}

	private static AddressRange CreateLocalNodeAddressRange(CommunicationsAddress localAddress)
	{
		return AddressRange.FromValues(
			CreateLocalNodeAddress(localAddress, 0),
			CreateLocalNodeAddress(localAddress, 63));
	}

	private static CommunicationsAddress CreateLocalNodeAddress(
		CommunicationsAddress localAddress,
		byte port)
	{
		return CommunicationsAddress.FromValues(
			localAddress.Brigade,
			localAddress.Node,
			Port.FromValue(PortIdentifier.FromValue(port)));
	}
}
