using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record Reset : IGD92MessageContents
{
	private static readonly MessageType ResetMessageType =
		MessageType.FromValue(GD92MessageType.Reset);

	private Reset(ResetReason resetReason)
	{
		this.ResetReason = resetReason;
	}

	public ResetReason ResetReason { get; }
	public MessageType Type => ResetMessageType;

	public static Reset FromFields(ResetReason resetReason)
	{
		ArgumentNullException.ThrowIfNull(resetReason);

		return new Reset(resetReason);
	}

	public static Reset FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(ResetReason.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.ResetReason.ToWireValue();
	}
}
