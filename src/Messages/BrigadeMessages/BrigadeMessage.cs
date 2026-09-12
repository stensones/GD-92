using Stensones.GD92.Fields;
using FieldText = Stensones.GD92.Fields.Text;

namespace Stensones.GD92.Messages;

public sealed record BrigadeMessage : IGD92MessageContents
{
	private static readonly MessageType BrigadeMessageType =
		MessageType.FromValue(GD92MessageType.BrigadeMessage);

	private BrigadeMessage(FieldText text)
	{
		this.Text = text;
	}

	public FieldText Text { get; }
	public MessageType Type => BrigadeMessageType;

	public static BrigadeMessage FromFields(FieldText text)
	{
		ArgumentNullException.ThrowIfNull(text);

		return new BrigadeMessage(text);
	}

	public static BrigadeMessage FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(FieldText.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.Text.ToWireValue();
	}
}
