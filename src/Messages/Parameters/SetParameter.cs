using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record SetParameter : IGD92MessageContents
{
	private static readonly MessageType SetParameterMessageType =
		MessageType.FromValue(GD92MessageType.SetParameter);

	private SetParameter(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue)
	{
		this.ParameterTable = parameterTable;
		this.ParameterNumber = parameterNumber;
		this.ParameterValue = parameterValue;
	}

	public ParameterTable ParameterTable { get; }
	public ParameterNumber ParameterNumber { get; }
	public ParameterValue ParameterValue { get; }
	public MessageType Type => SetParameterMessageType;

	public static SetParameter FromFields(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterValue parameterValue)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);
		ArgumentNullException.ThrowIfNull(parameterValue);

		return new SetParameter(parameterTable, parameterNumber, parameterValue);
	}

	public static SetParameter FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		var parameterTable = ParameterTable.FromEncodedMessageBuffer(ref buffer);
		var parameterNumber = ParameterNumber.FromEncodedMessageBuffer(ref buffer);
		var valueLength = buffer.RemainingBitCount;

		if (valueLength % 8 != 0)
		{
			throw new InvalidOperationException("Set Parameter Value must be byte-aligned.");
		}

		var parameterValue = new byte[valueLength / 8];

		for (var index = 0; index < parameterValue.Length; index++)
		{
			parameterValue[index] = (byte)buffer.ReadUnsignedBits(8);
		}

		return FromFields(
			parameterTable,
			parameterNumber,
			ParameterValue.FromWireValue(parameterValue));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.ParameterTable.ToWireValue(),
			.. this.ParameterNumber.ToWireValue(),
			.. this.ParameterValue.ToWireValue()
		];
	}
}
