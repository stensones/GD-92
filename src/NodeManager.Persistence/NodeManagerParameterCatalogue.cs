using Stensones.GD92.Fields;
using Stensones.GD92.Messages;

namespace NodeManager.Persistence;

public static class NodeManagerParameterCatalogue
{
	public static ParameterNumber PortNumber { get; } = ParameterNumber.FromValue(1);
	public static ParameterNumber AgentType { get; } = ParameterNumber.FromValue(2);
	public static ParameterNumber ControlAddress { get; } = ParameterNumber.FromValue(3);

	internal static ParameterValue EncodeControlAddress(CommunicationsAddress controlAddress)
	{
		ArgumentNullException.ThrowIfNull(controlAddress);

		return ParameterValue.FromWireValue(controlAddress.ToWireValue());
	}

	internal static CommunicationsAddress ReadControlAddress(ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterValue);

		var buffer = new EncodedMessageBuffer(parameterValue.ToWireValue());
		var controlAddress = CommunicationsAddress.FromEncodedMessageBuffer(ref buffer);
		if (buffer.RemainingBitCount != 0)
		{
			throw new ArgumentException(
				"Node Manager Parameter 3 contains trailing encoded data.",
				nameof(parameterValue));
		}

		return controlAddress;
	}
}
