using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
});

await builder.Build().RunAsync();
