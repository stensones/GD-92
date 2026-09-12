using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ResetRequest : IGD92MessageContents
{
	private static readonly MessageType ResetRequestMessageType =
		MessageType.FromValue(GD92MessageType.ResetRequest);

	private ResetRequest(ResetType resetType)
	{
		this.ResetType = resetType;
	}

	public ResetType ResetType { get; }
	public MessageType Type => ResetRequestMessageType;

	public static ResetRequest FromFields(ResetType resetType)
	{
		ArgumentNullException.ThrowIfNull(resetType);

		return new ResetRequest(resetType);
	}

	public static ResetRequest FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(ResetType.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.ResetType.ToWireValue();
	}
}
