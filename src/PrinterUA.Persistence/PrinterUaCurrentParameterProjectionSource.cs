namespace PrinterUA.Persistence;

public sealed class PrinterUaCurrentParameterProjectionSource
{
	private PrinterUaCurrentParameterProjection? currentParameters;

	public void Publish(PrinterUaCurrentParameterProjection projection)
	{
		ArgumentNullException.ThrowIfNull(projection);
		Interlocked.Exchange(ref this.currentParameters, projection);
	}

	public PrinterUaCurrentParameterProjection GetCurrent()
	{
		return Volatile.Read(ref this.currentParameters) ??
			throw new InvalidOperationException("Printer UA current Parameters have not been loaded.");
	}
}
