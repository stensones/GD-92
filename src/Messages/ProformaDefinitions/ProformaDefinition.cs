using Stensones.GD92.Fields;
using FieldTable = Stensones.GD92.Fields.Table;

namespace Stensones.GD92.Messages;

public sealed record ProformaDefinition : IGD92MessageContents
{
	private static readonly MessageType ProformaDefinitionMessageType =
		MessageType.FromValue(GD92MessageType.ProformaDefinition);

	private ProformaDefinition(FormatType formatType, FieldTable table)
	{
		this.FormatType = formatType;
		this.Table = table;
	}

	public FormatType FormatType { get; }
	public FieldTable Table { get; }
	public MessageType Type => ProformaDefinitionMessageType;

	public static ProformaDefinition FromFields(FormatType formatType, FieldTable table)
	{
		ArgumentNullException.ThrowIfNull(formatType);
		ArgumentNullException.ThrowIfNull(table);
		return new ProformaDefinition(formatType, table);
	}

	public static ProformaDefinition FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			FormatType.FromEncodedMessageBuffer(ref buffer),
			FieldTable.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue() => [.. this.FormatType.ToWireValue(), .. this.Table.ToWireValue()];
}
