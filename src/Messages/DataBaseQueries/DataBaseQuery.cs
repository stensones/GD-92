using Stensones.GD92.Fields;
using FieldText = Stensones.GD92.Fields.Text;

namespace Stensones.GD92.Messages;

public sealed record DataBaseQuery : IGD92MessageContents
{
	private static readonly MessageType DataBaseQueryMessageType =
		MessageType.FromValue(GD92MessageType.DatabaseQuery);

	private DataBaseQuery(QueryType queryType, FieldText text)
	{
		this.QueryType = queryType;
		this.Text = text;
	}

	public QueryType QueryType { get; }
	public FieldText Text { get; }
	public MessageType Type => DataBaseQueryMessageType;

	public static DataBaseQuery FromFields(QueryType queryType, FieldText text)
	{
		ArgumentNullException.ThrowIfNull(queryType);
		ArgumentNullException.ThrowIfNull(text);

		return new DataBaseQuery(queryType, text);
	}

	public static DataBaseQuery FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			QueryType.FromEncodedMessageBuffer(ref buffer),
			FieldText.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.QueryType.ToWireValue(),
			.. this.Text.ToWireValue()
		];
	}
}
