using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record MtaStatusChange : IGD92MessageContents
{
	private static readonly MessageType MtaStatusChangeMessageType =
		MessageType.FromValue(GD92MessageType.MtaStatusChange);

	private MtaStatusChange(MtaStatus mtaStatus)
	{
		this.MtaStatus = mtaStatus;
	}

	public MtaStatus MtaStatus { get; }
	public MessageType Type => MtaStatusChangeMessageType;

	public static MtaStatusChange FromFields(MtaStatus mtaStatus)
	{
		ArgumentNullException.ThrowIfNull(mtaStatus);

		return new MtaStatusChange(mtaStatus);
	}

	public static MtaStatusChange FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(MtaStatus.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.MtaStatus.ToWireValue();
	}
}
