using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class Acknowledgement : IGD92MessageContents
{
	private static readonly MessageType AcknowledgementMessageType =
		MessageType.FromValue(GD92MessageType.Acknowledgement);

	private Acknowledgement()
	{
	}

	public MessageType Type => AcknowledgementMessageType;

	public static Acknowledgement Create()
	{
		return new Acknowledgement();
	}

	public byte[] ToWireValue()
	{
		return [];
	}
}
