using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var application = builder.Build();
application.Services
	.GetRequiredService<ILoggerFactory>()
	.CreateLogger("PrinterUA")
	.LogInformation("Printer UA started.");

await application.RunAsync();
