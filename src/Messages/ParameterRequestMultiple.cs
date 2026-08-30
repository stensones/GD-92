using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ParameterRequestMultiple : IGD92MessageContents
{
	private static readonly MessageType ParameterRequestMultipleMessageType =
		MessageType.FromValue(GD92MessageType.ParameterRequestMultiple);

	private ParameterRequestMultiple(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterEntryIndex firstEntry,
		ParameterEntryIndex lastEntry)
	{
		this.ParameterTable = parameterTable;
		this.ParameterNumber = parameterNumber;
		this.FirstEntry = firstEntry;
		this.LastEntry = lastEntry;
	}

	public ParameterTable ParameterTable { get; }
	public ParameterNumber ParameterNumber { get; }
	public ParameterEntryIndex FirstEntry { get; }
	public ParameterEntryIndex LastEntry { get; }
	public MessageType Type => ParameterRequestMultipleMessageType;

	public static ParameterRequestMultiple FromFields(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterEntryIndex firstEntry,
		ParameterEntryIndex lastEntry)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);
		ArgumentNullException.ThrowIfNull(firstEntry);
		ArgumentNullException.ThrowIfNull(lastEntry);

		return new ParameterRequestMultiple(parameterTable, parameterNumber, firstEntry, lastEntry);
	}

	public static ParameterRequestMultiple FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			ParameterTable.FromEncodedMessageBuffer(ref buffer),
			ParameterNumber.FromEncodedMessageBuffer(ref buffer),
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer),
			ParameterEntryIndex.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.ParameterTable.ToWireValue(),
			.. this.ParameterNumber.ToWireValue(),
			.. this.FirstEntry.ToWireValue(),
			.. this.LastEntry.ToWireValue()
		];
	}
}
