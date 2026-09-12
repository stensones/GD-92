using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record AlertEng : IGD92MessageContents
{
	private static readonly MessageType AlertEngMessageType =
		MessageType.FromValue(GD92MessageType.AlertEng);

	private AlertEng(AlerterEngineering alerterEngineering)
	{
		this.AlerterEngineering = alerterEngineering;
	}

	public AlerterEngineering AlerterEngineering { get; }
	public MessageType Type => AlertEngMessageType;

	public static AlertEng FromFields(AlerterEngineering alerterEngineering)
	{
		ArgumentNullException.ThrowIfNull(alerterEngineering);

		return new AlertEng(alerterEngineering);
	}

	public static AlertEng FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(AlerterEngineering.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.AlerterEngineering.ToWireValue();
	}
}
