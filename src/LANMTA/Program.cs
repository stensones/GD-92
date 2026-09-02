using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var application = builder.Build();
application.Services
	.GetRequiredService<ILoggerFactory>()
	.CreateLogger("LANMTA")
	.LogInformation("LAN MTA started.");

await application.RunAsync();
