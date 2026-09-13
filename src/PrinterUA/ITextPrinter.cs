namespace PrinterUA;

public interface ITextPrinter
{
	Task PrintAsync(string text, CancellationToken cancellationToken);
}
