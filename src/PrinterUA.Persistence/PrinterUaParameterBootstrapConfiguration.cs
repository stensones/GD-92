using Stensones.GD92.Fields;

namespace PrinterUA.Persistence;

public sealed class PrinterUaParameterBootstrapConfiguration
{
	private PrinterUaParameterBootstrapConfiguration(
		CommunicationsAddress localAddress,
		CommunicationsAddress controlAddress)
	{
		this.LocalAddress = localAddress;
		this.ControlAddress = controlAddress;
	}

	public CommunicationsAddress LocalAddress { get; }
	public CommunicationsAddress ControlAddress { get; }

	public static PrinterUaParameterBootstrapConfiguration FromAddresses(
		CommunicationsAddress localAddress,
		CommunicationsAddress controlAddress)
	{
		ArgumentNullException.ThrowIfNull(localAddress);
		ArgumentNullException.ThrowIfNull(controlAddress);

		return new PrinterUaParameterBootstrapConfiguration(localAddress, controlAddress);
	}
}
