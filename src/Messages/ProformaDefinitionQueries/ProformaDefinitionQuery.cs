using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record ProformaDefinitionQuery : IGD92MessageContents
{
	private static readonly MessageType ProformaDefinitionQueryMessageType =
		MessageType.FromValue(GD92MessageType.ProformaDefinitionQuery);

	private ProformaDefinitionQuery(FormatType formatType)
	{
		this.FormatType = formatType;
	}

	public FormatType FormatType { get; }
	public MessageType Type => ProformaDefinitionQueryMessageType;

	public static ProformaDefinitionQuery FromFields(FormatType formatType)
	{
		ArgumentNullException.ThrowIfNull(formatType);
		return new ProformaDefinitionQuery(formatType);
	}

	public static ProformaDefinitionQuery FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer) =>
		FromFields(FormatType.FromEncodedMessageBuffer(ref buffer));

	public byte[] ToWireValue() => this.FormatType.ToWireValue();
}
