using Stensones.GD92.Fields;
using FieldTable = Stensones.GD92.Fields.Table;

namespace Stensones.GD92.Messages;

public sealed record FormattedText : IGD92MessageContents
{
	private static readonly MessageType FormattedTextMessageType =
		MessageType.FromValue(GD92MessageType.FormattedText);

	private FormattedText(FormatType formatType, FieldTable table)
	{
		this.FormatType = formatType;
		this.Table = table;
	}

	public FormatType FormatType { get; }
	public FieldTable Table { get; }
	public MessageType Type => FormattedTextMessageType;

	public static FormattedText FromFields(FormatType formatType, FieldTable table)
	{
		ArgumentNullException.ThrowIfNull(formatType);
		ArgumentNullException.ThrowIfNull(table);

		return new FormattedText(formatType, table);
	}

	public static FormattedText FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			FormatType.FromEncodedMessageBuffer(ref buffer),
			FieldTable.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.FormatType.ToWireValue(),
			.. this.Table.ToWireValue()
		];
	}
}
