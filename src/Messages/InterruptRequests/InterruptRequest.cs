using Stensones.GD92.Fields;
using FieldText = Stensones.GD92.Fields.Text;

namespace Stensones.GD92.Messages;

public sealed record InterruptRequest : IGD92MessageContents
{
	private static readonly MessageType InterruptRequestMessageType =
		MessageType.FromValue(GD92MessageType.InterruptRequest);

	private InterruptRequest(Callsign callsign, RequestCode requestCode, FieldText text)
	{
		this.Callsign = callsign;
		this.RequestCode = requestCode;
		this.Text = text;
	}

	public Callsign Callsign { get; }
	public RequestCode RequestCode { get; }
	public FieldText Text { get; }
	public MessageType Type => InterruptRequestMessageType;

	public static InterruptRequest FromFields(Callsign callsign, RequestCode requestCode, FieldText text)
	{
		ArgumentNullException.ThrowIfNull(callsign);
		ArgumentNullException.ThrowIfNull(requestCode);
		ArgumentNullException.ThrowIfNull(text);

		return new InterruptRequest(callsign, requestCode, text);
	}

	public static InterruptRequest FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(
			Callsign.FromEncodedMessageBuffer(ref buffer),
			RequestCode.FromEncodedMessageBuffer(ref buffer),
			FieldText.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return [
			.. this.Callsign.ToWireValue(),
			.. this.RequestCode.ToWireValue(),
			.. this.Text.ToWireValue()
		];
	}
}
