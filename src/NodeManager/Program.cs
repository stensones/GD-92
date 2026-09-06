using Microsoft.EntityFrameworkCore;
using NodeManager.Persistence;
using NodeManager.Router.Parameters;
using NodeManager.Router.Participants;
using ParticipantParameters;
using Stensones.GD92.Transport.RabbitMQ;
using Wolverine;
using Wolverine.RabbitMQ;

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

var requestSettings = RouterParameterRequestSettings.FromConfiguration(builder.Configuration);

builder.Services.AddWolverine(options =>
{
	options.UseRabbitMqUsingNamedConnection("RabbitMQ").AutoProvision();
	options.ListenForUserAgentIngress(requestSettings.MessageOriginator);
	options.ListenForLocalParticipantIngress(requestSettings.MessageOriginator);
});

builder.Services.AddSingleton(requestSettings);
builder.AddNpgsqlDbContext<NodeManagerDbContext>("node-manager-database");
builder.Services.AddScoped<IParticipantParameterStore, EfNodeManagerParameterStore>();
builder.Services.AddScoped<NodeManagerParameterBootstrapper>();
builder.Services.AddSingleton<NodeManagerCurrentParameterProjectionSource>();
builder.Services.AddSingleton<IPendingDeliveryRegistry, InMemoryPendingDeliveryRegistry>();
builder.Services.AddSingleton<IInventoryScanRegistry, InMemoryInventoryScanRegistry>();
builder.Services.AddSingleton<IInventoryScanRunner, InventoryScanRunner>();
builder.Services.AddSingleton<RouterParameterResponseReceiver>();
builder.Services.AddSingleton<IUserAgentIngressReceiver>(serviceProvider =>
	serviceProvider.GetRequiredService<RouterParameterResponseReceiver>());
builder.Services.AddScoped<ILocalParticipantIngressReceiver, NodeManagerParticipantIngressReceiver>();
builder.Services.AddScoped<IRouterIngress, NodeManagerRouterIngress>();
builder.Services.AddSingleton<INodeLoginRetryDelay, NodeLoginRetryDelay>();
builder.Services.AddSingleton<INodeLoginRetryScheduler>(serviceProvider =>
	new NodeLoginRetryScheduler(
		requestSettings.NodeLoginRetryPolicy,
		serviceProvider.GetRequiredService<IPendingDeliveryRegistry>(),
		serviceProvider.GetRequiredService<IServiceScopeFactory>(),
		serviceProvider.GetRequiredService<INodeLoginRetryDelay>(),
		serviceProvider.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping));
builder.Services.AddScoped<IRouterParameterRequestService, RouterParameterRequestService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
	var database = scope.ServiceProvider.GetRequiredService<NodeManagerDbContext>();
	await database.Database.MigrateAsync();

	var bootstrapper = scope.ServiceProvider.GetRequiredService<NodeManagerParameterBootstrapper>();
	var projection = await bootstrapper.LoadCurrentParameterProjectionAsync(
		NodeManagerParameterBootstrapConfiguration.FromAddress(requestSettings.MessageOriginator));
	app.Services.GetRequiredService<NodeManagerCurrentParameterProjectionSource>().Publish(projection);
}

app.UseStaticFiles(); // Enables serving static files from wwwroot
app.MapDefaultEndpoints();
app.MapControllers();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();