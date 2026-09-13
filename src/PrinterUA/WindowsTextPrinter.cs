using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.Versioning;
using Stensones.GD92.Fields;

namespace PrinterUA;

public sealed class WindowsTextPrinter(PrinterUaSettings settings) : ITextPrinter
{
	public Task PrintAsync(string text, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(text);
		cancellationToken.ThrowIfCancellationRequested();

		if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
		{
			throw new PlatformNotSupportedException("Printer UA requires Windows printing.");
		}

		var printerSettings = new PrinterSettings();
		if (!string.IsNullOrWhiteSpace(settings.PrinterHostName))
		{
			printerSettings.PrinterName = settings.PrinterHostName;
		}

		if (!printerSettings.IsValid)
		{
			throw new PrinterUnavailableException(PrinterReasonCode.OffLine);
		}

		PrintOnWindows(printerSettings, text);

		return Task.CompletedTask;
	}

	[SupportedOSPlatform("windows6.1")]
	private static void PrintOnWindows(PrinterSettings printerSettings, string text)
	{
		using var document = new PrintDocument
		{
			DocumentName = "GD-92 Text Message",
			PrinterSettings = printerSettings
		};
		document.PrintPage += (_, eventArguments) =>
		{
			using var font = new Font(FontFamily.GenericSansSerif, 10);
			(eventArguments.Graphics ?? throw new InvalidOperationException(
				"Windows did not provide a printer graphics surface."))
				.DrawString(text, font, Brushes.Black, eventArguments.MarginBounds);
		};
		document.Print();
	}
}
