using NodeManager.Router.Parameters;
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
});

builder.Services.AddSingleton(requestSettings);
builder.Services.AddSingleton<IPendingDeliveryRegistry, InMemoryPendingDeliveryRegistry>();
builder.Services.AddSingleton<IUserAgentIngressReceiver, RouterParameterResponseReceiver>();
builder.Services.AddScoped<IRouterIngress>(serviceProvider =>
	new RabbitMqRouterIngress(
		serviceProvider.GetRequiredService<IMessageBus>(),
		requestSettings.LocalRouter));
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

app.UseStaticFiles(); // Enables serving static files from wwwroot
app.MapDefaultEndpoints();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();