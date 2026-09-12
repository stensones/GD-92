using Stensones.GD92.Fields;
using FieldPrinterStatus = Stensones.GD92.Fields.PrinterStatus;

namespace Stensones.GD92.Messages;

public sealed record PrinterStatus : IGD92MessageContents
{
	private static readonly MessageType PrinterStatusMessageType =
		MessageType.FromValue(GD92MessageType.PrinterStatus);

	private PrinterStatus(FieldPrinterStatus status)
	{
		this.Status = status;
	}

	public FieldPrinterStatus Status { get; }
	public MessageType Type => PrinterStatusMessageType;

	public static PrinterStatus FromFields(FieldPrinterStatus status)
	{
		ArgumentNullException.ThrowIfNull(status);

		return new PrinterStatus(status);
	}

	public static PrinterStatus FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(FieldPrinterStatus.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.Status.ToWireValue();
	}
}
