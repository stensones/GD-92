using Stensones.GD92.Fields;

namespace PrinterUA;

public sealed class PrinterUnavailableException : Exception
{
	public PrinterUnavailableException(PrinterReasonCode reasonCode)
		: base($"The printer is unavailable: {reasonCode}.")
	{
		if (reasonCode is not PrinterReasonCode.OffLine and not PrinterReasonCode.NoPaper)
		{
			throw new ArgumentOutOfRangeException(nameof(reasonCode));
		}

		this.ReasonCode = reasonCode;
	}

	public PrinterReasonCode ReasonCode { get; }
}
