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

	public static Acknowledgement FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		if (buffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException("Acknowledgement Contents must be empty.");
		}

		return Create();
	}

	public byte[] ToWireValue()
	{
		return [];
	}
}
