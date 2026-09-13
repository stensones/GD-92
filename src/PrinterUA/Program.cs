using PrinterUA;
using PrinterUA.Persistence;
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
	Port.FromValue(PortIdentifier.FromValue(2)));
var localRouter = CommunicationsAddress.FromValues(
	localAddress.Brigade,
	localAddress.Node,
	Port.FromValue(PortIdentifier.FromValue(0)));
var controlAddress = CommunicationsAddress.FromValues(
	localAddress.Brigade,
	localAddress.Node,
	Port.FromValue(PortIdentifier.FromValue(25)));
var protocolVersion = ProtocolVersion.FromValue(ProtocolVersionNumber.FromValue(2));

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
	options.ListenForLocalParticipantIngress(localAddress);
});
builder.Services.AddSingleton(new PrinterUaSettings(
	localAddress,
	localRouter,
	controlAddress,
	protocolVersion,
	builder.Configuration["Printer:HostName"]));
builder.AddNpgsqlDbContext<PrinterUaDbContext>("printer-ua-database");
builder.Services.AddScoped<IParticipantParameterStore, EfPrinterUaParameterStore>();
builder.Services.AddScoped<PrinterUaParameterBootstrapper>();
builder.Services.AddSingleton<PrinterUaCurrentParameterProjectionSource>();
builder.Services.AddScoped<IRouterIngress, PrinterUaRouterIngress>();
builder.Services.AddSingleton<ITextPrinter, WindowsTextPrinter>();
builder.Services.AddScoped<PrinterUaTextMessageReceiver>();
builder.Services.AddScoped<ILocalParticipantIngressReceiver, PrinterUaParameterReceiver>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
	var database = scope.ServiceProvider.GetRequiredService<PrinterUaDbContext>();
	await database.Database.MigrateAsync();

	var settings = scope.ServiceProvider.GetRequiredService<PrinterUaSettings>();
	var bootstrapper = scope.ServiceProvider.GetRequiredService<PrinterUaParameterBootstrapper>();
	var projection = await bootstrapper.LoadCurrentParameterProjectionAsync(
		PrinterUaParameterBootstrapConfiguration.FromAddresses(
			settings.LocalAddress,
			settings.ControlAddress));
	host.Services.GetRequiredService<PrinterUaCurrentParameterProjectionSource>().Publish(projection);
}

await host.RunAsync();
