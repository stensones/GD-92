using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace PrinterUA.Persistence;

public sealed class PrinterUaCurrentParameterProjection
{
	private readonly ParameterValue portNumber;
	private readonly ParameterValue agentType;
	private readonly ParameterValue controlAddress;
	private readonly ParameterValue defaultSource;
	private readonly ParameterValue notifyPrinterAvailable;
	private readonly ParameterValue alternativeSource;
	private readonly ParameterValue reprintMessage;

	private PrinterUaCurrentParameterProjection(
		ParameterValue portNumber,
		ParameterValue agentType,
		ParameterValue controlAddress,
		ParameterValue defaultSource,
		ParameterValue notifyPrinterAvailable,
		ParameterValue alternativeSource,
		ParameterValue reprintMessage)
	{
		this.portNumber = portNumber;
		this.agentType = agentType;
		this.controlAddress = controlAddress;
		this.defaultSource = defaultSource;
		this.notifyPrinterAvailable = notifyPrinterAvailable;
		this.alternativeSource = alternativeSource;
		this.reprintMessage = reprintMessage;
	}

	public ParameterValue Get(ParameterNumber parameterNumber)
	{
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return parameterNumber == PrinterUaParameterCatalogue.PortNumber
			? this.portNumber
			: parameterNumber == PrinterUaParameterCatalogue.AgentType
				? this.agentType
				: parameterNumber == PrinterUaParameterCatalogue.ControlAddress
					? this.controlAddress
					: parameterNumber == PrinterUaParameterCatalogue.DefaultSource.Number
						? this.defaultSource
						: parameterNumber == PrinterUaParameterCatalogue.NotifyPrinterAvailable.Number
							? this.notifyPrinterAvailable
							: parameterNumber == PrinterUaParameterCatalogue.AlternativeSource.Number
								? this.alternativeSource
								: parameterNumber == PrinterUaParameterCatalogue.ReprintMessage.Number
									? this.reprintMessage
									: throw new ArgumentOutOfRangeException(
										nameof(parameterNumber),
										"Printer UA does not own the requested Parameter.");
	}

	public static PrinterUaCurrentParameterProjection FromNonVolatileParameters(
		ParameterValue portNumber,
		ParameterValue agentType,
		ParameterValue controlAddress,
		AddressRange defaultSource,
		ProtocolBoolean notifyPrinterAvailable,
		AddressTable alternativeSource,
		ProtocolBoolean reprintMessage)
	{
		ArgumentNullException.ThrowIfNull(portNumber);
		ArgumentNullException.ThrowIfNull(agentType);
		ArgumentNullException.ThrowIfNull(controlAddress);
		ArgumentNullException.ThrowIfNull(defaultSource);
		ArgumentNullException.ThrowIfNull(alternativeSource);

		return new PrinterUaCurrentParameterProjection(
			portNumber,
			agentType,
			controlAddress,
			PrinterUaParameterCatalogue.DefaultSource.Encode(defaultSource),
			PrinterUaParameterCatalogue.NotifyPrinterAvailable.Encode(notifyPrinterAvailable),
			PrinterUaParameterCatalogue.AlternativeSource.Encode(alternativeSource),
			PrinterUaParameterCatalogue.ReprintMessage.Encode(reprintMessage));
	}
}
