using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ParameterRequestMultiple : IGD92MessageContents
{
	private static readonly MessageType ParameterRequestMultipleMessageType =
		MessageType.FromValue(GD92MessageType.ParameterRequestMultiple);

	private ParameterRequestMultiple(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterEntrySelection entrySelection)
	{
		this.ParameterTable = parameterTable;
		this.ParameterNumber = parameterNumber;
		this.EntrySelection = entrySelection;
	}

	public ParameterTable ParameterTable { get; }
	public ParameterNumber ParameterNumber { get; }
	public ParameterEntrySelection EntrySelection { get; }
	public MessageType Type => ParameterRequestMultipleMessageType;

	public static ParameterRequestMultiple FromFields(
		ParameterTable parameterTable,
		ParameterNumber parameterNumber,
		ParameterEntrySelection entrySelection)
	{
		ArgumentNullException.ThrowIfNull(parameterTable);
		ArgumentNullException.ThrowIfNull(parameterNumber);
		ArgumentNullException.ThrowIfNull(entrySelection);

		return new ParameterRequestMultiple(parameterTable, parameterNumber, entrySelection);
	}

	public static ParameterRequestMultiple FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			ParameterTable.FromEncodedMessageBuffer(ref buffer),
			ParameterNumber.FromEncodedMessageBuffer(ref buffer),
			ParameterEntrySelection.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.ParameterTable.ToWireValue(),
			.. this.ParameterNumber.ToWireValue(),
			.. this.EntrySelection.ToWireValue()
		];
	}
}
