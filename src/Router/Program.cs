using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ParticipantParameters;
using Router;
using Router.Persistence;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddScoped<IRouterPasswordVerifierStore, EfRouterPasswordVerifierStore>();
builder.Services.AddScoped<RouterParameterBootstrapper>();
builder.Services.AddSingleton<RouterCurrentParameterProjectionSource>();
builder.Services.AddScoped(serviceProvider =>
	new RouterParameterRead(
		routerSettings.LocalAddress,
		routerSettings.ProtocolVersion,
		serviceProvider.GetRequiredService<RouterCurrentParameterProjectionSource>(),
		serviceProvider.GetRequiredService<IParticipantParameterStore>(),
		routerSettings.NodeName,
		routerSettings.MaximumMessageLength,
		routerSettings.NetworkManagerAddress1,
		routerSettings.NetworkManagerAddress2,
		routerSettings.ManualAcknowledgementTimeout));
builder.Services.AddScoped(serviceProvider =>
	new NodeLogin(
		routerSettings.LocalAddress,
		routerSettings.ProtocolVersion,
		serviceProvider.GetRequiredService<RouterCurrentParameterProjectionSource>()));
builder.Services.AddScoped(serviceProvider =>
	new Level1PasswordModification(
		routerSettings.LocalAddress,
		routerSettings.ProtocolVersion,
		serviceProvider.GetRequiredService<RouterCurrentParameterProjectionSource>(),
		serviceProvider.GetRequiredService<IRouterPasswordVerifierStore>()));
builder.Services.AddScoped(serviceProvider =>
	new RouterLocalDelivery(
		routerSettings.LocalAddress,
		serviceProvider.GetRequiredService<RouterParameterRead>(),
		serviceProvider.GetRequiredService<NodeLogin>(),
		serviceProvider.GetRequiredService<Level1PasswordModification>(),
		serviceProvider.GetRequiredService<IUserAgentIngress>(),
		serviceProvider.GetRequiredService<ILocalParticipantIngress>(),
		serviceProvider.GetRequiredService<ILogger<RouterLocalDelivery>>()));
builder.Services.AddScoped<IUserAgentIngress, RabbitMqUserAgentIngress>();
builder.Services.AddScoped<ILocalParticipantIngress, RabbitMqLocalParticipantIngress>();
builder.Services.AddScoped<IRouterIngressReceiver>(serviceProvider =>
	new RouterIngressReceiver(
		serviceProvider.GetRequiredService<RouterLocalDelivery>(),
		serviceProvider.GetRequiredService<ILogger<RouterIngressReceiver>>()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
	var database = scope.ServiceProvider.GetRequiredService<RouterDbContext>();
	await database.Database.MigrateAsync();

	var bootstrapper = scope.ServiceProvider.GetRequiredService<RouterParameterBootstrapper>();
	var projection = await bootstrapper.LoadCurrentParameterProjectionAsync(
		routerSettings.ParameterBootstrapConfiguration);
	app.Services.GetRequiredService<RouterCurrentParameterProjectionSource>().Publish(projection);
}

app.MapHealthChecks("/health");
await app.RunAsync();
