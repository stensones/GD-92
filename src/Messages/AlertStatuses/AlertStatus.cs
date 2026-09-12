using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record AlertStatus : IGD92MessageContents
{
	private static readonly MessageType AlertStatusMessageType =
		MessageType.FromValue(GD92MessageType.AlertStatus);

	private AlertStatus(AlerterStatus alerterStatus)
	{
		this.AlerterStatus = alerterStatus;
	}

	public AlerterStatus AlerterStatus { get; }
	public MessageType Type => AlertStatusMessageType;

	public static AlertStatus FromFields(AlerterStatus alerterStatus)
	{
		ArgumentNullException.ThrowIfNull(alerterStatus);

		return new AlertStatus(alerterStatus);
	}

	public static AlertStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(AlerterStatus.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.AlerterStatus.ToWireValue();
	}
}
