using Stensones.GD92.Fields;
using Stensones.GD92.Messages;
using Stensones.GD92.Transport.RabbitMQ;
using TextMessage = Stensones.GD92.Messages.Text;

namespace PrinterUA;

public sealed class PrinterUaTextMessageReceiver(
	PrinterUaSettings settings,
	ITextPrinter printer,
	IRouterIngress routerIngress)
{
	public async Task ReceiveAsync(Envelope envelope, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(envelope);

		if (envelope.Contents is not TextMessage text)
		{
			return;
		}

		try
		{
			await printer.PrintAsync(text.MessageText.Value, cancellationToken);
		}
		catch (PrinterUnavailableException exception)
			when (envelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested)
		{
			await routerIngress.SubmitAsync(
				Envelope.CreateNegativeAcknowledgement(
					envelope,
					settings.LocalAddress,
					settings.ProtocolVersion,
					envelope.Destinations,
					ReasonCode.FromPrinterReasonCode(exception.ReasonCode)),
				cancellationToken);
			return;
		}

		if (envelope.AcknowledgementAndSequence.AcknowledgementRequest.IsRequested)
		{
			await routerIngress.SubmitAsync(
				Envelope.CreateAcknowledgement(
					envelope,
					settings.LocalAddress,
					settings.ProtocolVersion),
				cancellationToken);
		}
	}
}
