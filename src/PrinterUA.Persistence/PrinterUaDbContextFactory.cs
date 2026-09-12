using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrinterUA.Persistence;

public sealed class PrinterUaDbContextFactory : IDesignTimeDbContextFactory<PrinterUaDbContext>
{
	public PrinterUaDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<PrinterUaDbContext>()
			.UseNpgsql("Host=localhost;Database=printer-ua;Username=postgres;******")
			.Options;

		return new PrinterUaDbContext(options);
	}
}
