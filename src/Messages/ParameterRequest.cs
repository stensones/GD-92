using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ParameterRequest : IGD92MessageContents
{
	private static readonly MessageType ParameterRequestMessageType =
		MessageType.FromValue(GD92MessageType.ParameterRequest);

	private ParameterRequest(ParameterTable parameterTable, ParameterNumber parameterNumber)
	{
		this.ParameterTable = parameterTable;
		this.ParameterNumber = parameterNumber;
	}

	public ParameterTable ParameterTable { get; }
	public ParameterNumber ParameterNumber { get; }
	public MessageType Type => ParameterRequestMessageType;

	public static ParameterRequest FromFields(ParameterTable parameterTable, ParameterNumber parameterNumber)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);

		return new ParameterRequest(parameterTable, parameterNumber);
	}

	public static ParameterRequest FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			ParameterTable.FromEncodedMessageBuffer(ref buffer),
			ParameterNumber.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [.. this.ParameterTable.ToWireValue(), .. this.ParameterNumber.ToWireValue()];
	}
}
