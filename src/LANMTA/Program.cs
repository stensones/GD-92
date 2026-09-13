using LANMTA;
using LANMTA.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParticipantParameters;
using Stensones.GD92.Fields;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var localAddress = CommunicationsAddress.FromValues(
	Brigade.FromValue(BrigadeOrAgencyIdentifier.FromValue(26)),
	Node.FromValue(NodeIdentifier.FromValue(100)),
	Port.FromValue(PortIdentifier.FromValue(1)));
var localRouter = CommunicationsAddress.FromValues(
	localAddress.Brigade,
	localAddress.Node,
	Port.FromValue(PortIdentifier.FromValue(0)));
var protocolVersion = ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2));

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
	options.ListenForLocalParticipantIngress(localAddress);
});
builder.Services.AddSingleton(new LanMtaSettings(localAddress, localRouter, protocolVersion));
builder.AddNpgsqlDbContext<LanMtaDbContext>("lan-mta-database");
builder.Services.AddScoped<IParticipantParameterStore, EfLanMtaParameterStore>();
builder.Services.AddScoped<LanMtaParameterBootstrapper>();
builder.Services.AddSingleton<LanMtaCurrentParameterProjectionSource>();
builder.Services.AddSingleton<ILanMtaRetainedParameterReader, LanMtaRetainedParameterReader>();
builder.Services.AddScoped<IRouterIngress, LanMtaRouterIngress>();
builder.Services.AddScoped<ILocalParticipantIngressReceiver, LanMtaParameterReceiver>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
	var database = scope.ServiceProvider.GetRequiredService<LanMtaDbContext>();
	await database.Database.MigrateAsync();

	var settings = scope.ServiceProvider.GetRequiredService<LanMtaSettings>();
	var bootstrapper = scope.ServiceProvider.GetRequiredService<LanMtaParameterBootstrapper>();
	var projection = await bootstrapper.LoadCurrentParameterProjectionAsync(
		LanMtaParameterBootstrapConfiguration.FromAddress(settings.LocalAddress));
	host.Services.GetRequiredService<LanMtaCurrentParameterProjectionSource>().Publish(projection);
}

await host.RunAsync();
