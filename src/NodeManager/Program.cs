using Microsoft.EntityFrameworkCore;
using NodeManager.Persistence;
using NodeManager.RealTime;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using ParticipantParameters;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

Extensions.PreferAspireAssignedPorts();
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services
	.AddControllersWithViews()
	.AddRazorRuntimeCompilation()
	.AddRazorOptions(options =>
	{
		// Adds the controller folder as a new location to look for views
		options.ViewLocationFormats.Add("/{1}/{0}.cshtml");
		options.ViewLocationFormats.Add("/SharedViews/{0}.cshtml"); // For shared views
	});
builder.Services.AddSignalR();

var requestSettings = RouterParameterRequestSettings.FromConfiguration(builder.Configuration);
var inventoryScanSettings = InventoryScanSettings.FromConfiguration(builder.Configuration);

if (!builder.Environment.IsEnvironment("Testing"))
{
	builder.Services.AddWolverine(options =>
	{
		options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
		options.ListenForUserAgentIngress(requestSettings.MessageOriginator);
		options.ListenForLocalParticipantIngress(requestSettings.MessageOriginator);
	});
}

builder.Services.AddSingleton(requestSettings);
builder.Services.AddSingleton(requestSettings.ManagementTransactionRetryPolicy);
builder.Services.AddSingleton(inventoryScanSettings);
builder.AddNpgsqlDbContext<NodeManagerDbContext>("node-manager-database");
builder.Services.AddScoped<IParticipantParameterStore, EfNodeManagerParameterStore>();
builder.Services.AddScoped<NodeManagerParameterBootstrapper>();
builder.Services.AddSingleton<NodeManagerCurrentParameterProjectionSource>();
builder.Services.AddSingleton<InventoryScan>();
builder.Services.AddSingleton<ManagementTransactions>();
builder.Services.AddSingleton<IRouterSessionAuthorization, RouterSessionAuthorization>();
builder.Services.AddSingleton<IManagementTransactionUiNotifier, SignalRManagementTransactionUiNotifier>();
builder.Services.AddSingleton<IInventoryScanUiNotifier, SignalRInventoryScanUiNotifier>();
builder.Services.AddSingleton<IManagementTransactionService>(serviceProvider =>
	serviceProvider.GetRequiredService<ManagementTransactions>());
builder.Services.AddSingleton<IUserAgentIngressReceiver>(serviceProvider =>
	serviceProvider.GetRequiredService<ManagementTransactions>());
builder.Services.AddScoped<ILocalParticipantIngressReceiver, NodeManagerParticipantIngressReceiver>();
builder.Services.AddScoped<IRouterIngress, NodeManagerRouterIngress>();
builder.Services.AddSingleton<IManagementTransactionRetryDelay, ManagementTransactionRetryDelay>();
builder.Services.AddScoped<IParticipantParameterRequestService, ParticipantParameterRequestService>();
builder.Services.AddScoped<IRouterParameterRequestService, RouterParameterRequestService>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
	await using var scope = app.Services.CreateAsyncScope();
	var database = scope.ServiceProvider.GetRequiredService<NodeManagerDbContext>();
	await database.Database.MigrateAsync();

	var bootstrapper = scope.ServiceProvider.GetRequiredService<NodeManagerParameterBootstrapper>();
	var projection = await bootstrapper.LoadCurrentParameterProjectionAsync(
		NodeManagerParameterBootstrapConfiguration.FromAddress(requestSettings.MessageOriginator));
	app.Services.GetRequiredService<NodeManagerCurrentParameterProjectionSource>().Publish(projection);
}

app.UseStaticFiles(); // Enables serving static files from wwwroot
app.UseMiddleware<BrowserSessionMiddleware>();
if (!app.Environment.IsDevelopment())
{
	app.MapHealthChecks("/health");
}

app.MapDefaultEndpoints();
app.MapControllers();
app.MapHub<ManagementTransactionHub>("/hubs/management-transactions");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public sealed class NodeManagerApplication;