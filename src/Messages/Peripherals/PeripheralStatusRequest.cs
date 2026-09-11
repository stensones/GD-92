using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed class PeripheralStatusRequest : IGD92MessageContents
{
	private static readonly MessageType PeripheralStatusRequestMessageType =
		MessageType.FromValue(GD92MessageType.PeripheralStatusRequest);

	private PeripheralStatusRequest()
	{
	}

	public MessageType Type => PeripheralStatusRequestMessageType;

	public static PeripheralStatusRequest Create()
	{
		return new PeripheralStatusRequest();
	}

	public static PeripheralStatusRequest FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		if (buffer.RemainingBitCount != 0)
		{
			throw new InvalidOperationException("Peripheral Status Request Contents must be empty.");
		}

		return Create();
	}

	public byte[] ToWireValue()
	{
		return [];
	}
}
