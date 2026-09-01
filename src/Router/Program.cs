using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Router;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
var routerSettings = RouterSettings.FromConfiguration(builder.Configuration);

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
	options.ListenForRouterIngress(routerSettings.LocalAddress);
});

builder.Services.AddSingleton(routerSettings);
builder.Services.AddSingleton(new RouterParameterRequestHandler(
	routerSettings.LocalAddress,
	routerSettings.ProtocolVersion));
builder.Services.AddSingleton<IRouterIngressReceiver, RouterIngressReceiver>();

await builder.Build().RunAsync();
