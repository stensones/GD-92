using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ParticipantParameters;
using Router;
using Router.Persistence;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
var routerSettings = RouterSettings.FromConfiguration(builder.Configuration);

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
	options.ListenForRouterIngress(routerSettings.LocalAddress);
});

builder.Services.AddSingleton(routerSettings);
builder.AddNpgsqlDbContext<RouterDbContext>("router-database");
builder.Services.AddScoped<IParticipantParameterStore, EfRouterParameterStore>();
builder.Services.AddScoped<IRouterLevel1PasswordVerifierStore, EfRouterPasswordVerifierStore>();
builder.Services.AddScoped<RouterParameterBootstrapper>();
builder.Services.AddSingleton<RouterCurrentParameterProjectionSource>();
builder.Services.AddScoped(serviceProvider =>
	new RouterParameterRequestHandler(
		routerSettings.LocalAddress,
		routerSettings.ProtocolVersion,
		serviceProvider.GetRequiredService<RouterCurrentParameterProjectionSource>(),
		serviceProvider.GetRequiredService<IRouterLevel1PasswordVerifierStore>()));
builder.Services.AddScoped<IUserAgentIngress, RabbitMqUserAgentIngress>();
builder.Services.AddScoped<ILocalParticipantIngress, RabbitMqLocalParticipantIngress>();
builder.Services.AddScoped<IRouterIngressReceiver, RouterIngressReceiver>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
	var database = scope.ServiceProvider.GetRequiredService<RouterDbContext>();
	await database.Database.MigrateAsync();

	var bootstrapper = scope.ServiceProvider.GetRequiredService<RouterParameterBootstrapper>();
	var projection = await bootstrapper.LoadCurrentParameterProjectionAsync(
		routerSettings.ParameterBootstrapConfiguration);
	host.Services.GetRequiredService<RouterCurrentParameterProjectionSource>().Publish(projection);
}

await host.RunAsync();
