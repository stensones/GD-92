using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace Stensones.GD92.Fields.Tests.Integration;

[Binding]
public sealed class DependencyInjectionHooks
{
	private static ServiceProvider? serviceProvider;

	[BeforeTestRun]
	public static void ConfigureServices()
	{
		var services = new ServiceCollection();
		services.AddGD92Fields();

		serviceProvider = services.BuildServiceProvider();
	}

	[AfterTestRun]
	public static void DisposeServices()
	{
		serviceProvider?.Dispose();
		serviceProvider = null;
	}
}
