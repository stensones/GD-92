var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();
// Add MVC services with Razor views support
builder.Services
	.AddControllersWithViews()
	.AddRazorRuntimeCompilation()
	.AddRazorOptions(options =>
	{
		// Adds the controller folder as a new location to look for views
		options.ViewLocationFormats.Add("/{1}/{0}.cshtml");
		options.ViewLocationFormats.Add("/SharedViews/{0}.cshtml"); // For shared views
	});

var app = builder.Build();

app.UseStaticFiles(); // Enables serving static files from wwwroot

//app.MapGet("/", () => "Hello, World! - Node Manager UA");

// co-locate a controllers views with the controller class itself, rather than in a separate Views folder
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();